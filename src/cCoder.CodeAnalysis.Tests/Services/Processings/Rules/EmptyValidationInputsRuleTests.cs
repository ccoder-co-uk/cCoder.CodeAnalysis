// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class EmptyValidationInputsRuleTests
{
    [Theory]
    [InlineData("ValidateInputs(inputs: []);")]
    [InlineData("ValidateInputs(inputs: Array.Empty<object>());")]
    [InlineData("ValidationRulesEngine.Validate(inputs: new object[0]);")]
    public void EmptyValidationInputsShouldBeRejected(string validationCall)
    {
        AnalysisItem[] results = Evaluate(methodBody: validationCall);

        results.Should().ContainSingle(result => result.Code == "STX0025");
    }

    [Fact]
    public void ParameterlessMethodWithoutValidationShouldBeAccepted()
    {
        AnalysisItem[] results = Evaluate(methodBody: "Execute();");

        results.Should().NotContain(result =>
            result.Code == "STX0011" || result.Code == "STX0025");
    }

    [Fact]
    public void MeaningfulParameterlessStateValidationShouldBeAccepted()
    {
        AnalysisItem[] results = Evaluate(methodBody: "ValidateMigrationState();");

        results.Should().NotContain(result => result.Code == "STX0025");
    }

    [Fact]
    public void NonEmptyValidationInputsShouldBeAccepted()
    {
        AnalysisItem[] results = Evaluate(
            parameter: "object value",
            methodBody: "ValidateInputs(inputs: [value]);");

        results.Should().NotContain(result => result.Code == "STX0025");
    }

    private static AnalysisItem[] Evaluate(
        string methodBody,
        string parameter = "")
    {
        string source =
            $"internal sealed partial class ExampleService {{ public void Execute({parameter}) {{ {methodBody} }} }}";

        TypeDeclarationSyntax declaration = CSharpSyntaxTree.ParseText(
                text: source,
                path: "ExampleService.cs")
            .GetRoot()
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Single();

        Class architectureElement = new()
        {
            Name = "Example.Services.Foundations.ExampleService",
            StandardElementType = StandardElementType.FoundationService,
            LineNumber = 1,
            Methods = [],
            AnalysisDeclarations = [declaration],
            AnalysisDependencies = [],
            AnalysisImplementedInterfaces = [],
        };

        EvaluationContext context = new()
        {
            ArchitectureElement = architectureElement,
            ArchitectureModel = new Architecture
            {
                Project = new ProjectMetadata { AssemblyName = "Example" },
                Classes = [architectureElement],
                Interfaces = [],
            },
        };

        return new STXRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();
    }
}