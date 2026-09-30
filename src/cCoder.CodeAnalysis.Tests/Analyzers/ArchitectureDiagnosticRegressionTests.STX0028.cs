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
    [Fact]
    public void CollectionReturningMethod_WhenSubjectIsSingular_IsReported()
    {
        Architecture architecture = AnalyzeCardinalityMethod(
            returnType: "System.Linq.IQueryable<Example.Models.Component>",
            methodName: "GetAllComponent"
        );

        Assert.Contains(collection: architecture.AnalysisItems, filter: item => item.Code == "STX0028");
    }

    [Fact]
    public void CollectionReturningMethod_WhenSubjectIsPlural_IsAccepted()
    {
        Architecture architecture = AnalyzeCardinalityMethod(
            returnType: "System.Linq.IQueryable<Example.Models.Component>",
            methodName: "GetAllComponents"
        );

        Assert.DoesNotContain(collection: architecture.AnalysisItems, filter: item => item.Code == "STX0028");
    }

    [Fact]
    public void SingularReturningMethod_WhenSubjectIsPlural_IsReported()
    {
        Architecture architecture = AnalyzeCardinalityMethod(
            returnType: "Example.Models.Component",
            methodName: "GetComponentsById"
        );

        Assert.Contains(collection: architecture.AnalysisItems, filter: item => item.Code == "STX0028");
    }

    [Fact]
    public void SingularReturningMethod_WhenSubjectIsSingular_IsAccepted()
    {
        Architecture architecture = AnalyzeCardinalityMethod(
            returnType: "Example.Models.Component",
            methodName: "GetComponentById"
        );

        Assert.DoesNotContain(collection: architecture.AnalysisItems, filter: item => item.Code == "STX0028");
    }

    [Fact]
    public async Task CardinalityMismatch_WhenAnalyzedByCompiler_IsReportedAtMethod()
    {
        SyntaxTree modelTree = CSharpSyntaxTree.ParseText(
            text: "namespace Example.Models; public sealed class Component { }",
            path: "Models/Component.cs"
        );
        SyntaxTree serviceTree = CSharpSyntaxTree.ParseText(
            text: """
            namespace Example.Services.Foundations;
            internal sealed class ComponentService
            {
                public Example.Models.Component[] GetAllComponent() => [];
            }
            """,
            path: "Services/Foundations/ComponentService.cs"
        );
        CSharpCompilation compilation = CreateCompilation(modelTree, serviceTree);

        ImmutableArray<Diagnostic> diagnostics = await compilation
            .WithAnalyzers(analyzers: ImmutableArray.Create<DiagnosticAnalyzer>(new ArchitectureDiagnosticAnalyzer()))
            .GetAnalyzerDiagnosticsAsync();

        Diagnostic diagnostic = Assert.Single(collection: diagnostics, predicate: item => item.Id == "STX0028");

        Assert.Equal(expected: serviceTree.FilePath, actual: diagnostic.Location.SourceTree!.FilePath);
        Assert.Equal(expected: 3, actual: diagnostic.Location.GetLineSpan().StartLinePosition.Line);
    }

    [Fact]
    public void ReferencedCCoderModel_WhenSubjectIsSingularForCollection_IsReported()
    {
        CSharpCompilation modelCompilation = CSharpCompilation.Create(
            assemblyName: "cCoder.Data",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    text: "namespace cCoder.Data.Models.CMS; public sealed class Component { }",
                    path: "Models/CMS/Component.cs"
                ),
            ],
            references: [MetadataReference.CreateFromFile(path: typeof(object).Assembly.Location)],
            options: new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary)
        );

        using MemoryStream modelAssembly = new();
        modelCompilation.Emit(peStream: modelAssembly);

        CSharpCompilation serviceCompilation = CSharpCompilation.Create(
            assemblyName: "cCoder.ContentManagement",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    text: """
                    namespace cCoder.ContentManagement.Services.Foundations;
                    internal sealed class ComponentService
                    {
                        public cCoder.Data.Models.CMS.Component[] GetAllComponent() => [];
                    }
                    """,
                    path: "Services/Foundations/ComponentService.cs"
                ),
            ],
            references:
            [
                MetadataReference.CreateFromFile(path: typeof(object).Assembly.Location),
                MetadataReference.CreateFromImage(peImage: modelAssembly.ToArray()),
            ],
            options: new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary)
        );

        Architecture architecture = ArchitectureAnalysis.Generate(compilation: serviceCompilation);

        Assert.Contains(collection: architecture.AnalysisItems, filter: item => item.Code == "STX0028");
    }

    [Theory]
    [InlineData(
        "System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Example.Models.Category>>",
        "GetCategoriesAsync"
    )]
    [InlineData("System.Threading.Tasks.ValueTask<Example.Models.Category>", "GetCategoryAsync")]
    [InlineData("Example.Models.Status[]", "GetStatuses")]
    [InlineData("System.Collections.Generic.IEnumerable<Example.Models.Box>", "GetBoxes")]
    [InlineData("System.Collections.Generic.IEnumerable<Example.Models.Batch>", "GetBatches")]
    [InlineData("System.Collections.Generic.IEnumerable<Example.Models.Person>", "GetPeople")]
    [InlineData("System.Collections.Generic.IEnumerable<Example.Models.Analysis>", "GetAnalyses")]
    [InlineData("Microsoft.AspNetCore.Mvc.ActionResult<Example.Models.Component[]>", "GetComponents")]
    [InlineData("Example.Models.Metadata", "GetMetadata")]
    [InlineData("System.Collections.Generic.IEnumerable<Example.Models.Metadata>", "GetMetadata")]
    public void CardinalityNaming_WhenWrappedOrEnglishEndingVaries_IsAccepted(string returnType, string methodName)
    {
        Architecture architecture = AnalyzeCardinalityMethod(returnType: returnType, methodName: methodName);

        Assert.DoesNotContain(collection: architecture.AnalysisItems, filter: item => item.Code == "STX0028");
    }

    [Theory]
    [InlineData("string", "GetComponent")]
    [InlineData("byte[]", "GetComponents")]
    [InlineData("System.Collections.Generic.IEnumerable<string>", "GetStrings")]
    [InlineData("Example.Models.Component", "ResolveById")]
    public void CardinalityNaming_WhenReturnSubjectIsNotApplicable_IsIgnored(string returnType, string methodName)
    {
        Architecture architecture = AnalyzeCardinalityMethod(returnType: returnType, methodName: methodName);

        Assert.DoesNotContain(collection: architecture.AnalysisItems, filter: item => item.Code == "STX0028");
    }

    private static Architecture AnalyzeCardinalityMethod(string returnType, string methodName)
    {
        SyntaxTree modelTree = CSharpSyntaxTree.ParseText(
            text: """
            namespace Example.Models
            {
                public sealed class Component { }
                public sealed class Category { }
                public sealed class Status { }
                public sealed class Box { }
                public sealed class Batch { }
                public sealed class Person { }
                public sealed class Analysis { }
                public sealed class Metadata { }
            }

            namespace Microsoft.AspNetCore.Mvc
            {
                public class ActionResult<T> { }
            }
            """,
            path: "Models/Subjects.cs"
        );

        SyntaxTree serviceTree = CSharpSyntaxTree.ParseText(
            text: $$"""
            namespace Example.Services.Foundations;
            internal sealed class ComponentService
            {
                public {{returnType}} {{methodName}}() => throw new System.NotImplementedException();
            }
            """,
            path: "Services/Foundations/ComponentService.cs"
        );

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Example",
            syntaxTrees: [modelTree, serviceTree],
            references:
            [
                MetadataReference.CreateFromFile(path: typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(path: typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(path: typeof(IQueryable<>).Assembly.Location),
                MetadataReference.CreateFromFile(path: typeof(Task<>).Assembly.Location),
            ],
            options: new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary)
        );

        return ArchitectureAnalysis.Generate(compilation: compilation);
    }
}