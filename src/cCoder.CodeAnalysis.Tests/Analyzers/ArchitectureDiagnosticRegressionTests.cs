// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using System.Collections.Immutable;
using cCoder.CodeAnalysis.Analyzers;
using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace cCoder.CodeAnalysis.Tests.Analyzers;

public sealed partial class ArchitectureDiagnosticRegressionTests
{
    [Theory]
    [InlineData("Example.Models")]
    [InlineData("Example.Models.Graph")]
    public void ShouldNotTreatADependencyDtoAsAnExternalResource(string modelNamespace)
    {
        // Given
        CSharpCompilation compilation = CreateCompilation(
            CSharpSyntaxTree.ParseText(
                text: $"namespace {modelNamespace}; public sealed class Dependency {{ public string FromType {{ get; set; }} public string ToType {{ get; set; }} }}",
                path: "Models/Dependency.cs"));

        // When
        Architecture architecture = ArchitectureAnalysis.Generate(compilation: compilation);

        // Then
        Assert.Equal(expected: StandardElementType.Model, actual: Assert.Single(collection: architecture.Classes).StandardElementType);
        Assert.DoesNotContain(collection: architecture.AnalysisItems, filter: item => item.Code == "STXD002");
    }

    [Fact]
    public void ShouldStillRejectAnInvalidExplicitDependency()
    {
        // Given
        CSharpCompilation compilation = CreateCompilation(
            CSharpSyntaxTree.ParseText(
                text: "namespace Example.Dependencies; public sealed class StorageDependency { public string Name { get; set; } }",
                path: "Dependencies/StorageDependency.cs"));

        // When
        Architecture architecture = ArchitectureAnalysis.Generate(compilation: compilation);

        // Then
        Assert.Contains(collection: architecture.AnalysisItems, filter: item => item.Code == "STXD002");
    }

    [Theory]
    [InlineData(false, "STXFORMAT005")]
    [InlineData(true, "STXFORMAT005")]
    [InlineData(false, "STXM001")]
    [InlineData(true, "STXM001")]
    public async Task ShouldLocateDiagnosticsInTheOffendingPartialFileAsync(bool reverseTrees, string diagnosticCode)
    {
        // Given
        string typeNamespace = diagnosticCode == "STXM001" ? "Example.Models" : "Example.Exposures";
        SyntaxTree first = CSharpSyntaxTree.ParseText(
            text: $"namespace {typeNamespace}; public partial class Split {{ public int Good {{ get; set; }} }}" + new string('\n', 100),
            path: "Exposures/Split.First.cs");
        SyntaxTree second = CSharpSyntaxTree.ParseText(
            text: $"namespace {typeNamespace};\npublic partial class Split\n{{\npublic int Bad() => System.Math.Abs(-1);\n}}",
            path: "Exposures/Split.Second.cs");
        CSharpCompilation compilation = reverseTrees
            ? CreateCompilation(second, first)
            : CreateCompilation(first, second);

        // When
        ImmutableArray<Diagnostic> diagnostics = await compilation.WithAnalyzers(
            analyzers: ImmutableArray.Create<DiagnosticAnalyzer>(new ArchitectureDiagnosticAnalyzer()))
            .GetAnalyzerDiagnosticsAsync();

        // Then
        Diagnostic diagnostic = Assert.Single(collection: diagnostics, predicate: item => item.Id == diagnosticCode);
        Assert.Equal(expected: second.FilePath, actual: diagnostic.Location.SourceTree!.FilePath);
        Assert.Equal(expected: 3, actual: diagnostic.Location.GetLineSpan().StartLinePosition.Line);
    }

    private static CSharpCompilation CreateCompilation(params SyntaxTree[] trees) =>
        CSharpCompilation.Create(
            assemblyName: "Example",
            syntaxTrees: trees,
            references: new[] { MetadataReference.CreateFromFile(path: typeof(object).Assembly.Location) },
            options: new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary));
}