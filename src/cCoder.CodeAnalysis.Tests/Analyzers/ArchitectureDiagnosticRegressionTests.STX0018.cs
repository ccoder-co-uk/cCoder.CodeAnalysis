// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Analyzers;
using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis.CSharp;

namespace cCoder.CodeAnalysis.Tests.Analyzers;

public sealed partial class ArchitectureDiagnosticRegressionTests
{
    [Theory]
    [InlineData("Create")]
    [InlineData("Read")]
    [InlineData("Update")]
    [InlineData("Delete")]
    [InlineData("Insert")]
    [InlineData("Add")]
    [InlineData("Modify")]
    [InlineData("Remove")]
    [InlineData("Get")]
    [InlineData("Post")]
    [InlineData("Put")]
    [InlineData("Destroy")]
    public void ShouldRequireModelNamesForCrudVerbs(string verb)
    {
        // Given / When
        var bare = AnalyzeServiceMethod(methodName: verb);
        var bareAsync = AnalyzeServiceMethod(methodName: verb + "Async");
        var named = AnalyzeServiceMethod(methodName: verb + "School");
        var namedAsync = AnalyzeServiceMethod(methodName: verb + "SchoolAsync");
        // Then
        Assert.Contains(collection: bare.AnalysisItems, filter: item => item.Code == "STX0018");
        Assert.Contains(collection: bareAsync.AnalysisItems, filter: item => item.Code == "STX0018");
        Assert.DoesNotContain(collection: named.AnalysisItems, filter: item => item.Code == "STX0018");
        Assert.DoesNotContain(collection: namedAsync.AnalysisItems, filter: item => item.Code == "STX0018");
    }

    [Theory]
    [InlineData("GenerateDiagram")]
    [InlineData("GenerateDiagramAsync")]
    [InlineData("RenderRequest")]
    [InlineData("RenderRequestAsync")]
    [InlineData("Split")]
    [InlineData("PopulateTypes")]
    [InlineData("Parse")]
    [InlineData("Prepare")]
    [InlineData("Address")]
    [InlineData("Readjust")]
    [InlineData("Getter")]
    [InlineData("Postprocess")]
    public void ShouldNotRequireParameterModelNamesForNonCrudOperations(string methodName)
    {
        // Given / When
        Architecture architecture = AnalyzeServiceMethod(methodName: methodName);
        // Then
        Assert.DoesNotContain(collection: architecture.AnalysisItems, filter: item => item.Code == "STX0018");
    }

    [Theory]
    [InlineData("Render", true)]
    [InlineData("RenderAsync", true)]
    [InlineData("Render", false)]
    [InlineData("RenderAsync", false)]
    [InlineData("Generate", true)]
    [InlineData("GenerateAsync", true)]
    [InlineData("Generate", false)]
    [InlineData("GenerateAsync", false)]
    public void ShouldRequireASubjectForRenderAndGenerateOperations(string methodName, bool hasModelParameter)
    {
        // Given / When
        Architecture architecture = AnalyzeServiceMethod(methodName: methodName, hasModelParameter: hasModelParameter);
        // Then
        Assert.Contains(collection: architecture.AnalysisItems, filter: item => item.Code == "STX0018");
    }

    private static Architecture AnalyzeServiceMethod(string methodName, bool hasModelParameter = true)
    {
        string parameter = hasModelParameter ? "Example.Models.School school" : "";
        var compilation = CreateCompilation(CSharpSyntaxTree.ParseText(text: $$"""
            namespace Example.Models { public class School { public string Id { get; set; } } }
            namespace Example.Services.Processings
            {
                public class SchoolProcessingService
                {
                    public void {{methodName}}({{parameter}}) { }
                }
            }
            """, path: "Services/Processings/SchoolProcessingService.cs"));
        return ArchitectureAnalysis.Generate(compilation: compilation);
    }
}