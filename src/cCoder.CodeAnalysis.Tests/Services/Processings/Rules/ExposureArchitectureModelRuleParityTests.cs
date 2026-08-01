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
        new STXAPIRulesProcessingService().Evaluate(context: context)
            .Select(item => item.Code)
            .Should().Contain(["STXAPI001", "STXAPI002", "STXAPI003"]);
    }

    private static EvaluationContext CreateContext(
        string typeName,
        bool isApiController,
        IReadOnlyList<TypeDependency> dependencies,
        IReadOnlyList<string>? publicApiModelTypes = null)
    {
        Class element = new()
        {
            Name = typeName,
            StandardElementType = isApiController
                ? StandardElementType.HttpExposure
                : StandardElementType.Exposure,
            Properties = [],
            Methods = [],
            AnalysisDependencies = dependencies,
            AnalysisImplementedInterfaces = [],
            AnalysisIsApiController = isApiController,
            AnalysisPublicApiModelTypes = publicApiModelTypes ?? [],
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

    private static TypeDependency CreateDependency(
        StandardElementType standardElementType) =>
        new()
        {
            StandardElementType = standardElementType,
            TypeName = $"I{standardElementType}Dependency",
        };
}
