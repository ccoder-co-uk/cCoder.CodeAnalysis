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

    private static EvaluationContext CreateContext(string source)
    {
        TypeDeclarationSyntax declaration = CSharpSyntaxTree
            .ParseText(text: source)
            .GetRoot()
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Single();
        Class model = new() { Name = "Model", AnalysisDeclarations = [declaration] };

        return new EvaluationContext
        {
            ArchitectureElement = model,
            ArchitectureModel = new Architecture { Classes = [model] },
        };
    }
}