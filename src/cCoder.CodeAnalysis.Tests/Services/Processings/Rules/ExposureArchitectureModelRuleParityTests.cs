// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class ExposureArchitectureModelRuleParityTests
{
    [Fact]
    public void ExposureDependencyRulesShouldUseAttachedModelFacts()
    {
        EvaluationContext context = CreateContext(
            typeName: "StudentProvider",
            isApiController: false,
            dependencies:
            [
                CreateDependency(StandardElementType.FoundationService),
                CreateDependency(StandardElementType.ProcessingService),
                CreateDependency(StandardElementType.Broker),
            ]);
        context.IsApiController = true;
        context.Dependencies = [];

        new STXERulesProcessingService().Evaluate(context: context)
            .Select(item => item.Code)
            .Should().Contain(["STXE003", "STXE004"]);
    }

    [Fact]
    public void ApiRulesShouldUseAttachedModelFacts()
    {
        EvaluationContext context = CreateContext(
            typeName: "InvalidStudentsEndpoint",
            isApiController: true,
            dependencies: [],
            publicApiModelTypes: ["Student", "Teacher"]);
        context.IsApiController = false;
        context.Dependencies = [CreateDependency(StandardElementType.FoundationService)];
        context.PublicApiModelTypes = [];

        new STXAPIRulesProcessingService().Evaluate(context: context)
            .Select(item => item.Code)
            .Should().Contain(["STXAPI001", "STXAPI002", "STXAPI003"]);
    }

    private static EvaluationContext CreateContext(
        string typeName,
        bool isApiController,
        IReadOnlyList<TypeDependency> dependencies,
        IReadOnlyList<string>? publicApiModelTypes = null) =>
        new()
        {
            TypeName = typeName,
            Declarations = [],
            Dependencies = dependencies,
            ImplementedInterfaces = [],
            PublicApiModelTypes = publicApiModelTypes ?? [],
            ArchitectureElement = new Class
            {
                AnalysisDependencies = dependencies,
                AnalysisImplementedInterfaces = [],
                AnalysisIsApiController = isApiController,
                AnalysisPublicApiModelTypes = publicApiModelTypes ?? [],
            },
        };

    private static TypeDependency CreateDependency(
        StandardElementType standardElementType) =>
        new()
        {
            StandardElementType = standardElementType,
            TypeName = $"I{standardElementType}Dependency",
        };
}
