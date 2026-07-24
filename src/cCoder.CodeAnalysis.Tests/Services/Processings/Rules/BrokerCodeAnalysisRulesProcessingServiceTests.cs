// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class STXBRulesProcessingServiceTests
{
    [Fact]
    public void EvaluateShouldExecuteAllConfiguredBrokerRules()
    {
        TypeDeclarationSyntax declaration = CSharpSyntaxTree
            .ParseText(
                "// ---------------------------------------------------------------\r\n// Copyright (c) Coalition of the Good-Hearted Engineers\r\n// FREE TO USE TO CONNECT THE WORLD\r\n// ---------------------------------------------------------------\r\n\r\nclass Broker { void Run() { if (true) { for (;;) { try { } catch { } } } } }"
            )
            .GetRoot()
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Single();
        EvaluationContext evaluationContext = new EvaluationContext();
        evaluationContext.TypeName = "Example.Broker";
        evaluationContext.StandardElementType = StandardElementType.Broker;
        evaluationContext.ImplementedInterfaces = ["Example.IBroker"];
        evaluationContext.Declarations = [declaration];
        evaluationContext.Dependencies =
        [
            new TypeDependency { StandardElementType = StandardElementType.Exposure },
            new TypeDependency { StandardElementType = StandardElementType.Dependency },
        ];
        EvaluationContext context = evaluationContext;
        STXBRulesProcessingService service = new STXBRulesProcessingService();
        AnalysisItem[] results = service.Evaluate(context).ToArray();
        results
            .Select((AnalysisItem result) => result.Code)
            .Should()
            .BeEquivalentTo("STXB001", "STXB002", "STXB003", "STXB005");
    }
}
