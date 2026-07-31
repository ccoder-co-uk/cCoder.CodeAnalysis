// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Architectures;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class STXERulesProcessingServiceTests
{
    [Theory]
    [InlineData("AIConfigurationExtensions")]
    [InlineData("AIConfigurationProviderExtensions")]
    public void EvaluateShouldRejectMismatchedConfigurationExtensions(
        string typeName)
    {
        TypeDeclarationSyntax declaration = ParseDeclaration(
            """
            public static class ConfigurationExtensions
            {
                public static void Configure(this object configuration)
                {
                    if (IsEnabled())
                    {
                        AddProvider();
                    }

                    AddDefaults();
                }
            }
            """);

        EvaluationContext context = CreateContext(
            declaration,
            $"Example.Models.{typeName}");
        STXERulesProcessingService service = new();

        AnalysisItem[] results = service
            .Evaluate(context)
            .ToArray();

        results.Should()
            .ContainSingle(
                predicate: item =>
                    item.Code == "STXE007");
    }

    [Fact]
    public void EvaluateShouldRejectExtensionsTypeWithoutExtensionMethods()
    {
        TypeDeclarationSyntax declaration = ParseDeclaration(
            """
            internal static class AuthorizationExtensions
            {
                internal static void Authorize(object user)
                {
                }
            }
            """);

        EvaluationContext context = CreateContext(
            declaration: declaration,
            typeName: "Example.Extensions.AuthorizationExtensions");

        STXERulesProcessingService service = new();

        service.Evaluate(context: context)
            .Should()
            .ContainSingle(
                predicate: item => item.Code == "STXE006");
    }

    [Fact]
    public void EvaluateShouldRejectExtensionForDifferentReceiver()
    {
        TypeDeclarationSyntax declaration = ParseDeclaration(
            """
            internal static class MailConfigurationExtensions
            {
                internal static void Configure(
                    this MailConfiguration configuration)
                {
                }

                internal static void AddProvider(
                    this ProviderConfiguration configuration)
                {
                }
            }
            """);

        EvaluationContext context = CreateContext(
            declaration: declaration,
            typeName:
                "Example.Extensions.MailConfigurationExtensions");

        STXERulesProcessingService service = new();

        service.Evaluate(context: context)
            .Should()
            .ContainSingle(
                predicate: item => item.Code == "STXE007");
    }

    [Fact]
    public void EvaluateShouldAllowInterfaceReceiverNameWithoutInterfacePrefix()
    {
        TypeDeclarationSyntax declaration = ParseDeclaration(
            """
            internal static class ServiceCollectionExtensions
            {
                internal static void AddMail(
                    this IServiceCollection services)
                {
                }
            }
            """);

        EvaluationContext context = CreateContext(
            declaration: declaration,
            typeName:
                "Example.Extensions.ServiceCollectionExtensions");

        STXERulesProcessingService service = new();

        service.Evaluate(context: context)
            .Should()
            .NotContain(
                predicate: item => item.Code == "STXE007");
    }

    [Fact]
    public void EvaluateShouldAllowProviderClientOperationServices()
    {
        TypeDeclarationSyntax declaration = ParseDeclaration(
            """
            internal sealed class MicrosoftGraphMailClient
                : IMailClient
            {
            }
            """);

        EvaluationContext context = CreateContext(
            declaration: declaration,
            typeName:
                "cCoder.Mail.Providers.Exposures.MicrosoftGraphMailClient");

        context.ProjectName = "cCoder.Mail.Providers";
        context.ImplementedInterfaces = ["IMailClient"];
        context.Dependencies =
        [
            new TypeDependency
            {
                TypeName = "IMailSenderService",
                StandardElementType =
                    StandardElementType.FoundationService
            },
            new TypeDependency
            {
                TypeName = "IMailReceiverService",
                StandardElementType =
                    StandardElementType.FoundationService
            }
        ];
        context.ArchitectureElement!.AnalysisImplementedInterfaces = context.ImplementedInterfaces;
        context.ArchitectureElement.AnalysisDependencies = context.Dependencies;

        STXERulesProcessingService service = new();

        service.Evaluate(context: context)
            .Should()
            .NotContain(
                predicate: item => item.Code == "STXE003");
    }

    [Fact]
    public void EvaluateShouldStillRejectOrdinaryExposureWithTwoServices()
    {
        TypeDeclarationSyntax declaration = ParseDeclaration(
            """
            internal sealed class MailExposure
            {
            }
            """);

        EvaluationContext context = CreateContext(
            declaration: declaration,
            typeName: "cCoder.Mail.Exposures.MailExposure");

        context.ProjectName = "cCoder.Mail";
        context.ImplementedInterfaces = [];
        context.Dependencies =
        [
            new TypeDependency
            {
                TypeName = "IMailSenderService",
                StandardElementType =
                    StandardElementType.FoundationService
            },
            new TypeDependency
            {
                TypeName = "IMailReceiverService",
                StandardElementType =
                    StandardElementType.FoundationService
            }
        ];
        context.ArchitectureElement!.AnalysisImplementedInterfaces = context.ImplementedInterfaces;
        context.ArchitectureElement.AnalysisDependencies = context.Dependencies;

        STXERulesProcessingService service = new();

        service.Evaluate(context: context)
            .Should()
            .ContainSingle(
                predicate: item => item.Code == "STXE003");
    }

    [Fact]
    public void EvaluateShouldAllowMvcActionResponseFlow()
    {
        // given
        TypeDeclarationSyntax declaration = ParseDeclaration(
            """
            public class HomeController
            {
                public async Task<IActionResult> Get()
                {
                    if (await IsReadyAsync())
                    {
                        return Redirect("/");
                    }

                    return View();
                }
            }
            """);

        EvaluationContext context = CreateContext(declaration: declaration);
        STXERulesProcessingService service = new();

        // when
        AnalysisItem[] results = service
            .Evaluate(context: context)
            .ToArray();

        // then
        results
            .Should()
            .NotContain(
                predicate: result =>
                    result.Code == "STXE001"
                    || result.Code == "STXE005");
    }

    [Fact]
    public void EvaluateShouldAnalysePrivateControllerHelpers()
    {
        // given
        TypeDeclarationSyntax declaration = ParseDeclaration(
            """
            public class HomeController
            {
                private string BuildSession()
                {
                    if (IsReady())
                    {
                        LoadUser();
                    }

                    LoadTheme();
                    return "session";
                }
            }
            """);

        EvaluationContext context = CreateContext(declaration: declaration);
        STXERulesProcessingService service = new();

        // when
        string[] codes = service
            .Evaluate(context: context)
            .Select(selector: result => result.Code)
            .ToArray();

        // then
        codes
            .Should()
            .Contain(expected: "STXE001");

        codes
            .Should()
            .Contain(expected: "STXE005");
    }

    private static EvaluationContext CreateContext(
        TypeDeclarationSyntax declaration,
        string typeName = "Example.Controllers.HomeController")
    {
        Class architectureElement = new()
        {
            Name = typeName,
            StandardElementType = StandardElementType.Exposure,
            Properties = [],
            Methods = [],
            AnalysisDependencies = [],
            AnalysisImplementedInterfaces = [],
            AnalysisTypeFacts = ArchitectureProcessingService.CreateTypeAnalysisFacts(
                declarations: [declaration]),
        };

        return new EvaluationContext
        {
            TypeName = typeName,
            StandardElementType = StandardElementType.Exposure,
            ArchitectureElement = architectureElement,
            Dependencies = [],
        };
    }

    private static TypeDeclarationSyntax ParseDeclaration(
        string source) =>
        CSharpSyntaxTree
            .ParseText(text: source)
            .GetRoot()
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Single();
}
