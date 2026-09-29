// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Collections.Immutable;
using cCoder.CodeAnalysis.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace cCoder.CodeAnalysis.Tests.Analyzers;

public sealed class CrossProjectSourceAnalyzerTests
{
    [Fact]
    public async Task FileScopedUsing_WhenDeclaredOnItsConsumer_IsNotReportedAsync()
    {
        string projectDirectory = Path.Combine(
            path1: Path.GetTempPath(),
            path2: "CurrentProject");

        SyntaxTree source = CSharpSyntaxTree.ParseText(
            text: "using System; namespace Example.Models; public sealed class Model { }",
            path: Path.Combine(
                path1: projectDirectory,
                path2: "Models",
                path3: "Model.cs"));

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(
            source: source,
            projectDirectory: projectDirectory);

        Assert.DoesNotContain(
            collection: diagnostics,
            filter: diagnostic => diagnostic.Id == "STXSTRUCT005");
    }

    [Fact]
    public async Task GlobalUsingDirective_WhenDeclared_IsReportedAsync()
    {
        string projectDirectory = Path.Combine(
            path1: Path.GetTempPath(),
            path2: "CurrentProject");

        SyntaxTree source = CSharpSyntaxTree.ParseText(
            text: "global using System; namespace Example.Models; public sealed class Model { }",
            path: Path.Combine(
                path1: projectDirectory,
                path2: "Models",
                path3: "Model.cs"));

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(
            source: source,
            projectDirectory: projectDirectory);

        Assert.Contains(
            collection: diagnostics,
            filter: diagnostic => diagnostic.Id == "STXSTRUCT005");
    }

    [Fact]
    public async Task ImplicitUsings_WhenEnabledForProject_AreReportedAsync()
    {
        string projectDirectory = Path.Combine(
            path1: Path.GetTempPath(),
            path2: "CurrentProject");

        SyntaxTree source = CSharpSyntaxTree.ParseText(
            text: "namespace Example.Models; public sealed class Model { }",
            path: Path.Combine(
                path1: projectDirectory,
                path2: "Models",
                path3: "Model.cs"));

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(
            source: source,
            projectDirectory: projectDirectory,
            implicitUsings: "enable");

        Assert.Contains(
            collection: diagnostics,
            filter: diagnostic => diagnostic.Id == "STXSTRUCT005");
    }

    [Fact]
    public async Task SourceFile_WhenNamedGlobalUsings_IsReportedAsync()
    {
        string projectDirectory = Path.Combine(
            path1: Path.GetTempPath(),
            path2: "CurrentProject");

        SyntaxTree source = CSharpSyntaxTree.ParseText(
            text: "using System;",
            path: Path.Combine(
                path1: projectDirectory,
                path2: "GlobalUsings.cs"));

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(
            source: source,
            projectDirectory: projectDirectory);

        Assert.Contains(
            collection: diagnostics,
            filter: diagnostic => diagnostic.Id == "STXSTRUCT005");
    }

    [Fact]
    public async Task SourceFile_WhenCompiledFromOutsideProjectDirectory_IsReportedAsync()
    {
        string projectDirectory = Path.Combine(
            path1: Path.GetTempPath(),
            path2: "CurrentProject");

        SyntaxTree linkedSource = CSharpSyntaxTree.ParseText(
            text: "namespace Example.Models; public sealed class SharedModel { }",
            path: Path.Combine(
                path1: Path.GetTempPath(),
                path2: "Shared",
                path3: "SharedModel.cs"));

        ImmutableArray<Diagnostic> diagnostics = await GetDiagnosticsAsync(
            source: linkedSource,
            projectDirectory: projectDirectory);

        Assert.Contains(
            collection: diagnostics,
            filter: diagnostic => diagnostic.Id == "STXSTRUCT004");
    }

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(
        SyntaxTree source,
        string projectDirectory,
        string implicitUsings = "disable")
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Example",
            syntaxTrees: [source],
            references: [MetadataReference.CreateFromFile(path: typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(
                outputKind: OutputKind.DynamicallyLinkedLibrary));

        AnalyzerOptions analyzerOptions = new(
            additionalFiles: [],
            optionsProvider: new ProjectDirectoryAnalyzerConfigOptionsProvider(
                projectDirectory: projectDirectory,
                implicitUsings: implicitUsings));

        return await compilation
            .WithAnalyzers(
                analyzers: [new ArchitectureDiagnosticAnalyzer()],
                options: analyzerOptions)
            .GetAnalyzerDiagnosticsAsync();
    }

    private sealed class ProjectDirectoryAnalyzerConfigOptionsProvider(
        string projectDirectory,
        string implicitUsings)
        : AnalyzerConfigOptionsProvider
    {
        private readonly AnalyzerConfigOptions globalOptions =
            new ProjectDirectoryAnalyzerConfigOptions(
                projectDirectory: projectDirectory,
                implicitUsings: implicitUsings);

        public override AnalyzerConfigOptions GlobalOptions => globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) =>
            EmptyAnalyzerConfigOptions.Instance;

        public override AnalyzerConfigOptions GetOptions(
            AdditionalText textFile) =>
            EmptyAnalyzerConfigOptions.Instance;
    }

    private sealed class ProjectDirectoryAnalyzerConfigOptions(
        string projectDirectory,
        string implicitUsings)
        : AnalyzerConfigOptions
    {
        public override bool TryGetValue(
            string key,
            out string value)
        {
            if (key == "build_property.MSBuildProjectDirectory")
            {
                value = projectDirectory;
                return true;
            }

            if (key == "build_property.ImplicitUsings")
            {
                value = implicitUsings;
                return true;
            }

            value = string.Empty;
            return false;
        }
    }

    private sealed class EmptyAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        internal static readonly EmptyAnalyzerConfigOptions Instance = new();

        public override bool TryGetValue(
            string key,
            out string value)
        {
            value = string.Empty;
            return false;
        }
    }
}
