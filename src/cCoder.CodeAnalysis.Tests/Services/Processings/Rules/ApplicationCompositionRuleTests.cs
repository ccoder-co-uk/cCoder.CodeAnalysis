// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class ApplicationCompositionRuleTests
{
    private readonly STXAPPRulesProcessingService service = new STXAPPRulesProcessingService();

    [Fact]
    public void ConsoleCommandApplicationWithoutHostExtensionsEvaluatesAsExpected()
    {
        EvaluationContext context = CreateContext(
            typeName: "School.Cli.Program",
            sourceCode: "await host.RunAsync(args);",
            isConsoleApplication: true,
            projectTypeNames: ["School.Cli.Program"]
        );

        AnalysisItem[] items = service.Evaluate(context: context).ToArray();

        items.Should().ContainSingle(item => item.Code == "STXAPP006", "");
    }

    [Fact]
    public void ConsoleCommandApplicationWithHostExtensionsEvaluatesAsExpected()
    {
        EvaluationContext context = CreateContext(
            typeName: "School.Cli.Program",
            sourceCode: "await host.RunAsync(args);",
            isConsoleApplication: true,
            projectTypeNames: ["School.Cli.Program", "School.Cli.IHostExtensions"]
        );

        AnalysisItem[] items = service.Evaluate(context: context).ToArray();

        items.Should().NotContain(item => item.Code == "STXAPP006", "");
    }

    [Fact]
    public void HostExtensionsWithoutProviderRoutingEvaluatesAsExpected()
    {
        EvaluationContext context = CreateContext(
            typeName: "School.Cli.IHostExtensions",
            sourceCode:
                """
                public static class IHostExtensions
                {
                    public static ValueTask RunAsync(this IHost host, string[] arguments) =>
                        ValueTask.CompletedTask;
                }
                """
        );

        AnalysisItem[] items = service.Evaluate(context: context).ToArray();

        items.Should().ContainSingle(item => item.Code == "STXAPP007", "");
    }

    [Fact]
    public void HostExtensionsWithProviderRoutingEvaluatesAsExpected()
    {
        EvaluationContext context = CreateContext(
            typeName: "School.Cli.IHostExtensions",
            sourceCode:
                """
                public static class IHostExtensions
                {
                    public static ValueTask RunAsync(this IHost host, string[] arguments)
                    {
                        ICommandProcessingService service =
                            host.Services.GetRequiredService<ICommandProcessingService>();

                        return service.ProcessCommandAsync(arguments);
                    }
                }
                """
        );

        AnalysisItem[] items = service.Evaluate(context: context).ToArray();

        items.Should().NotContain(item => item.Code == "STXAPP007", "");
    }

    [Fact]
    public void ChainedServiceCollectionRegistrationsEvaluateAsExpected()
    {
        EvaluationContext context = CreateContext(
            typeName: "School.Cli.IServiceCollectionExtensions",
            sourceCode:
                """
                public static class IServiceCollectionExtensions
                {
                    public static void AddSchool(this IServiceCollection services)
                    {
                        services
                            .AddSingleton<IStudentBroker, StudentBroker>()
                            .AddSingleton<IStudentService, StudentService>();
                    }
                }
                """
        );

        AnalysisItem[] items = service.Evaluate(context: context).ToArray();

        items.Should().ContainSingle(item => item.Code == "STXAPP008", "");
    }

    [Fact]
    public void LayeredServiceCollectionRegistrationsEvaluateAsExpected()
    {
        EvaluationContext context = CreateContext(
            typeName: "School.Cli.IServiceCollectionExtensions",
            sourceCode:
                """
                public static class IServiceCollectionExtensions
                {
                    public static void AddSchool(this IServiceCollection services)
                    {
                        AddSchoolBrokers(services);
                        AddSchoolFoundations(services);
                    }

                    private static void AddSchoolBrokers(IServiceCollection services)
                    {
                        services.AddSingleton<IStudentBroker, StudentBroker>();
                    }

                    private static void AddSchoolFoundations(IServiceCollection services)
                    {
                        services.AddSingleton<IStudentService, StudentService>();
                    }
                }
                """
        );

        AnalysisItem[] items = service.Evaluate(context: context).ToArray();

        items.Should().NotContain(
            item => item.Code == "STXAPP008" || item.Code == "STXAPP009",
            ""
        );
    }

    [Fact]
    public void UnlayeredServiceCollectionRegistrationsEvaluateAsExpected()
    {
        EvaluationContext context = CreateContext(
            typeName: "School.Cli.IServiceCollectionExtensions",
            sourceCode:
                """
                public static class IServiceCollectionExtensions
                {
                    public static void AddSchool(this IServiceCollection services)
                    {
                        services.AddSingleton<IStudentService, StudentService>();
                    }
                }
                """
        );

        AnalysisItem[] items = service.Evaluate(context: context).ToArray();

        items.Should().ContainSingle(item => item.Code == "STXAPP009", "");
    }

    [Fact]
    public void RootBuilderOptionsPartialEvaluatesAsExpected()
    {
        EvaluationContext context = CreateContext(
            typeName: "School.Cli.CoreApiBuilderOptions",
            sourceCode:
                """
                public partial class CoreApiBuilderOptions
                {
                }
                """,
            filePath: "School.Cli/CoreApiBuilderOptions.Configuration.cs"
        );

        AnalysisItem[] items = service.Evaluate(context: context).ToArray();

        items.Should().NotContain(item => item.Code == "STXAPP001", "");
    }

    private static EvaluationContext CreateContext(
        string typeName,
        string sourceCode,
        bool isConsoleApplication = false,
        IReadOnlyCollection<string>? projectTypeNames = null,
        string? filePath = null
    )
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            text: sourceCode,
            path: filePath ?? $"{typeName.Split(separator: ['.']).Last()}.cs"
        );

        return new EvaluationContext
        {
            TypeName = typeName,
            StandardElementType = StandardElementType.App,
            ProjectName = "School.Cli",
            FilePath = syntaxTree.FilePath,
            SourceCode = sourceCode,
            IsConsoleApplication = isConsoleApplication,
            ProjectTypeNames = projectTypeNames ?? [typeName],
            Declarations = syntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .ToArray(),
            UsingNamespaces = [],
        };
    }
}