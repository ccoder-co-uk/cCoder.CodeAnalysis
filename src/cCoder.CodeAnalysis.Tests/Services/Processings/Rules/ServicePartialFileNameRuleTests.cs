// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class ServicePartialFileNameRuleTests
{
    [Theory]
    [InlineData("ExampleService.Standard.Validations.cs", "STX0008")]
    [InlineData("ExampleService.Standard.Exceptions.cs", "STX0009")]
    public void PartialWithAdditionalStandardSegmentShouldBeRejected(
        string partialFileName,
        string expectedCode)
    {
        EvaluationContext context = CreateContext(partialFileName: partialFileName);

        AnalysisItem[] results = new STXRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        results.Should().ContainSingle(result => result.Code == expectedCode);
    }

    [Theory]
    [InlineData("ExampleService.Validations.cs", "STX0008")]
    [InlineData("ExampleService.Exceptions.cs", "STX0009")]
    public void ExactlyNamedPartialShouldBeAccepted(
        string partialFileName,
        string unexpectedCode)
    {
        EvaluationContext context = CreateContext(partialFileName: partialFileName);

        AnalysisItem[] results = new STXRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        results.Should().NotContain(result => result.Code == unexpectedCode);
    }

    [Theory]
    [InlineData("EventService.Validations.cs", "STX0008")]
    [InlineData("EventService.Exceptions.cs", "STX0009")]
    public void GenericServiceShouldUseRuntimeSafePartialFileName(
        string partialFileName,
        string unexpectedCode)
    {
        EvaluationContext context = CreateContext(
            partialFileName: partialFileName,
            typeName: "Example.Services.Foundations.EventService<T>");

        new STXRulesProcessingService().Evaluate(context: context)
            .Should().NotContain(result => result.Code == unexpectedCode);
    }

    private static EvaluationContext CreateContext(
        string partialFileName,
        string typeName = "Example.Services.Foundations.ExampleService")
    {
        TypeDeclarationSyntax mainDeclaration = ParseDeclaration(
            fileName: "ExampleService.cs");

        TypeDeclarationSyntax partialDeclaration = ParseDeclaration(
            fileName: partialFileName);

        Class architectureElement = new()
        {
            Name = typeName,
            StandardElementType = StandardElementType.FoundationService,
            LineNumber = 1,
            Methods = [],
            AnalysisDeclarations = [mainDeclaration, partialDeclaration],
            AnalysisDependencies = [],
            AnalysisImplementedInterfaces = [],
        };

        return new EvaluationContext
        {
            ArchitectureElement = architectureElement,
            ArchitectureModel = new Architecture
            {
                Project = new ProjectMetadata { AssemblyName = "Example" },
                Classes = [architectureElement],
                Interfaces = [],
            },
        };
    }

    private static TypeDeclarationSyntax ParseDeclaration(string fileName) =>
        CSharpSyntaxTree.ParseText(
                text: "internal sealed partial class ExampleService { }",
                path: fileName)
            .GetRoot()
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Single();
}
