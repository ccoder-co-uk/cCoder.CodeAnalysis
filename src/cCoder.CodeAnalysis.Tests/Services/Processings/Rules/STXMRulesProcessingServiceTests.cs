// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class STXMRulesProcessingServiceTests
{
    [Fact]
    public void STXM001ShouldRejectEveryExplicitMethodLikeMember()
    {
        const string source = """
            internal sealed class Model(int value)
            {
                static Model() { }
                public Model() : this(0) { }
                ~Model() { }
                public void Execute() { }
                public override string ToString() => value.ToString();
                public static Model operator +(Model left, Model right) => left;
                public static explicit operator string(Model model) => model.ToString();
            }
            """;

        EvaluationContext context = CreateContext(source: source);

        AnalysisItem[] diagnostics = new STXMRulesProcessingService()
            .Evaluate(context: context)
            .Where(item => item.Code == "STXM001")
            .ToArray();

        diagnostics.Should().HaveCount(8, "primary and conventional method declarations are forbidden");
        diagnostics.Should().OnlyContain(item => item.Type == "Model");
    }

    [Fact]
    public void STXM001ShouldAcceptPropertyOnlyModel()
    {
        EvaluationContext context = CreateContext(
            source: "internal sealed class Model { public string Name { get; set; } = string.Empty; }"
        );

        new STXMRulesProcessingService().Evaluate(context: context)
            .Should().NotContain(item => item.Code == "STXM001");
    }

    [Fact]
    public void STXM001ShouldAllowConfigurationModelConstructor()
    {
        EvaluationContext context = CreateContext(
            source: "internal sealed class EventingConfiguration { public EventingConfiguration() { } }",
            typeName: "EventingConfiguration");

        new STXMRulesProcessingService().Evaluate(context: context)
            .Should().NotContain(item => item.Code == "STXM001");
    }

    [Fact]
    public void STXMRules_WhenModelIsException_DoNotApplyDataCarrierRules()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "internal sealed class StudentServiceException : InvalidOperationException "
                + "{ public StudentServiceException() { } public string Detail { get; set; } = string.Empty; }",
            typeName: "StudentServiceException");
        context.ArchitectureElement.BaseType = new TypeReference
        {
            Name = "InvalidOperationException",
            FullName = "System.InvalidOperationException",
        };
        context.ArchitectureElement.AnalysisIsException = true;

        // When
        AnalysisItem[] diagnostics = new STXMRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        diagnostics.Should().NotContain(
            item => item.Code.StartsWith(value: "STXM", comparisonType: StringComparison.Ordinal),
            "exception contracts are diagram models, not data-carrier models");
    }

    private static EvaluationContext CreateContext(
        string source,
        string typeName = "Model")
    {
        TypeDeclarationSyntax declaration = CSharpSyntaxTree
            .ParseText(text: source)
            .GetRoot()
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Single();
        Class model = new() { Name = typeName, AnalysisDeclarations = [declaration] };

        return new EvaluationContext
        {
            ArchitectureElement = model,
            ArchitectureModel = new Architecture { Classes = [model] },
        };
    }
}
