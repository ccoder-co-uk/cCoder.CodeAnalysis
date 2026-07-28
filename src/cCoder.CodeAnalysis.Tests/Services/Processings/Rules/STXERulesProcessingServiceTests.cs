// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
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
    public void EvaluateShouldIgnoreConfigurationExtensions(
        string typeName)
    {
        TypeDeclarationSyntax declaration = ParseDeclaration(
            """
            public static class ConfigurationExtensions
            {
                public static void Configure()
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

        results.Should().BeEmpty("");
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
        string typeName = "Example.Controllers.HomeController") =>
        new()
        {
            TypeName = typeName,
            StandardElementType = StandardElementType.Exposure,
            Declarations = [declaration],
            Dependencies = []
        };

    private static TypeDeclarationSyntax ParseDeclaration(
        string source) =>
        CSharpSyntaxTree
            .ParseText(text: source)
            .GetRoot()
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Single();
}
