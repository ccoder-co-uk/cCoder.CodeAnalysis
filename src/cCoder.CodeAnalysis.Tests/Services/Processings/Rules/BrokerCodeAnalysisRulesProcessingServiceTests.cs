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
        TypeDependency[] dependencies =
        [
            new TypeDependency { StandardElementType = StandardElementType.Exposure },
            new TypeDependency { StandardElementType = StandardElementType.Dependency },
        ];
        EvaluationContext context = CreateContext(
            typeName: "Example.Broker",
            declarations: [declaration],
            dependencies: dependencies,
            implementedInterfaces: ["Example.IBroker"]);
        STXBRulesProcessingService service = new STXBRulesProcessingService();
        AnalysisItem[] results = service.Evaluate(context).ToArray();
        results
            .Select((AnalysisItem result) => result.Code)
            .Should()
            .BeEquivalentTo("STXB001", "STXB002", "STXB003", "STXB005");
    }

    [Fact]
    public void EvaluateShouldAllowStronglyTypedConfigurationDependencies()
    {
        EvaluationContext context = CreateContext(
            typeName: "Example.Brokers.ExternalBroker",
            declarations: [],
            implementedInterfaces: ["Example.Brokers.IExternalBroker"],
            dependencies:
            [
                new TypeDependency
                {
                    TypeName = "Example.Models.ExampleConfiguration",
                    StandardElementType = StandardElementType.Model
                }
            ]);

        STXBRulesProcessingService service = new();

        AnalysisItem[] results = service
            .Evaluate(context: context)
            .ToArray();

        results
            .Should()
            .NotContain(predicate: result => result.Code == "STXB006");
    }

    private static EvaluationContext CreateContext(
        string typeName,
        IReadOnlyList<TypeDeclarationSyntax> declarations,
        IReadOnlyList<TypeDependency> dependencies,
        IReadOnlyList<string> implementedInterfaces)
    {
        Class element = new()
        {
            Name = typeName,
            StandardElementType = StandardElementType.Broker,
            Properties = [],
            Methods = [],
            AnalysisDeclarations = declarations,
            AnalysisDependencies = dependencies,
            AnalysisImplementedInterfaces = implementedInterfaces,
            AnalysisTypeFacts = new TypeAnalysisFacts(),
        };

        return new EvaluationContext
        {
            ArchitectureElement = element,
            ArchitectureModel = new Architecture
            {
                Project = new ProjectMetadata { AssemblyName = "Example" },
                Classes = [element],
            },
        };
    }
}
