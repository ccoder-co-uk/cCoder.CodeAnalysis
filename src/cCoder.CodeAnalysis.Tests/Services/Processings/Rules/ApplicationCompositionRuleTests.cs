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
                        services.AddBrokers();
                        services.AddFoundations();
                    }

                    private static void AddBrokers(
                        this IServiceCollection services)
                    {
                        services.AddSingleton<IStudentBroker, StudentBroker>();
                    }

                    private static void AddFoundations(
                        this IServiceCollection services)
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

    [Theory]
    [InlineData("cCoder.Data", "AddData")]
    [InlineData("cCoder.Security.Data", "AddSecurityData")]
    public void SupportingDataRegistrationEvaluatesAsExpected(
        string projectName,
        string methodName)
    {
        EvaluationContext context = CreateContext(
            typeName: $"{projectName}.IServiceCollectionExtensions",
            sourceCode:
                $$"""
                public static class IServiceCollectionExtensions
                {
                    public static void {{methodName}}(
                        this IServiceCollection services,
                        DataConfiguration configuration)
                    {
                        services.AddDependencies();
                    }

                    private static void AddDependencies(
                        this IServiceCollection services)
                    {
                    }
                }
                """,
            projectName: projectName);

        AnalysisItem[] items = service.Evaluate(context).ToArray();

        items.Should().NotContain(item =>
            item.Code == "STXAPP002"
            || item.Code == "STXAPP009");
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
    public void NonServiceCollectionMethodEvaluatesAsExpected()
    {
        EvaluationContext context = CreateContext(
            typeName: "School.Cli.IServiceCollectionExtensions",
            sourceCode:
                """
                public static class IServiceCollectionExtensions
                {
                    public static void AddCli(
                        this IServiceCollection services,
                        Action<CliConfiguration> configure)
                    {
                        services.AddBrokers();
                    }

                    private static void AddBrokers(
                        this IServiceCollection services)
                    {
                    }

                    public static WebApplication MapApp(
                        this WebApplication app) => app;
                }
                """);

        AnalysisItem[] items = service.Evaluate(context).ToArray();

        items.Should().ContainSingle(item => item.Code == "STXAPP010", "");
    }

    [Fact]
    public void ConventionalAppEntryPointEvaluatesAsExpected()
    {
        EvaluationContext context = CreateContext(
            typeName: "School.Cli.IServiceCollectionExtensions",
            sourceCode:
                """
                public static class IServiceCollectionExtensions
                {
                    public static void AddCli(
                        this IServiceCollection services,
                        IConfiguration applicationConfiguration,
                        Action<CliConfiguration> configure)
                    {
                        CliConfiguration configuration = new();
                        applicationConfiguration.Bind(configuration);
                        configure(configuration);
                        services.AddBrokers();
                    }

                    private static void AddBrokers(
                        this IServiceCollection services)
                    {
                    }
                }
                """);

        AnalysisItem[] items = service.Evaluate(context).ToArray();

        items.Should().NotContain(item =>
            item.Code == "STXAPP009"
            || item.Code == "STXAPP010"
            || item.Code == "STXAPP011"
            || item.Code == "STXAPP012");
    }

    [Fact]
    public void AppEntryPointWithoutConfigurationCallbackEvaluatesAsExpected()
    {
        EvaluationContext context = CreateContext(
            typeName: "School.Cli.IServiceCollectionExtensions",
            sourceCode:
                """
                public static class IServiceCollectionExtensions
                {
                    public static void AddCli(
                        this IServiceCollection services,
                        IConfiguration applicationConfiguration)
                    {
                        services.AddBrokers();
                    }

                    private static void AddBrokers(
                        this IServiceCollection services)
                    {
                    }
                }
                """);

        AnalysisItem[] items = service.Evaluate(context).ToArray();

        items.Should().ContainSingle(item => item.Code == "STXAPP012", "");
    }

    [Fact]
    public void DictionaryConfigurationPropertyEvaluatesAsExpected()
    {
        EvaluationContext context = CreateContext(
            typeName: "School.Cli.Models.CliConfiguration",
            sourceCode:
                """
                public sealed class CliConfiguration
                {
                    public Dictionary<string, string> Services { get; set; }
                }
                """);

        AnalysisItem[] items = service.Evaluate(context).ToArray();

        items.Should().ContainSingle(item => item.Code == "STXAPP013", "");
    }

    [Fact]
    public void ProgramOwnedConfigurationBindingEvaluatesAsExpected()
    {
        EvaluationContext context = CreateContext(
            typeName: "School.Cli.Program",
            sourceCode:
                """
                public sealed class Program
                {
                    public static void Main()
                    {
                        CliConfiguration configuration = new();
                        builder.Configuration.Bind(configuration);
                        builder.Services.AddCli(configuration);
                    }
                }
                """);

        AnalysisItem[] items = service.Evaluate(context).ToArray();

        items.Should().ContainSingle(item => item.Code == "STXAPP014", "");
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
        string? filePath = null,
        string projectName = "School.Cli"
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
            ProjectName = projectName,
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