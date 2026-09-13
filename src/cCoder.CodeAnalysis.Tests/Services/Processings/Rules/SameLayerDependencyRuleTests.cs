// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class SameLayerDependencyRuleTests
{
    [Theory]
    [InlineData(StandardElementType.Broker)]
    [InlineData(StandardElementType.FoundationService)]
    [InlineData(StandardElementType.ProcessingService)]
    [InlineData(StandardElementType.OrchestrationService)]
    [InlineData(StandardElementType.CoordinationService)]
    [InlineData(StandardElementType.ManagementService)]
    [InlineData(StandardElementType.AggregationService)]
    [InlineData(StandardElementType.Exposure)]
    public void ArchitecturalElement_WhenDependingOnSameLayer_IsReported(
        StandardElementType elementType)
    {
        EvaluationContext context = CreateContext(
            elementType: elementType,
            dependencyType: elementType,
            dependencyName: $"Example.I{elementType}");

        new STXRulesProcessingService()
            .Evaluate(context: context)
            .Should()
            .ContainSingle(item => item.Code == "STX0004");
    }

    [Fact]
    public void HttpExposure_WhenDependingOnPublicExposureContract_IsNotReported()
    {
        EvaluationContext context = CreateContext(
            elementType: StandardElementType.HttpExposure,
            dependencyType: StandardElementType.Exposure,
            dependencyName: "Example.Exposures.Templates.ITemplateManager");

        context.ArchitectureElement.IsPublic = true;
        context.ArchitectureElement.AnalysisIsApiController = true;
        context.ArchitectureModel.Interfaces.Add(item: new Class
        {
            Name = "Example.Exposures.Templates.ITemplateManager",
            IsPublic = true,
            Kind = ArchitectureTypeKind.Interface,
            StandardElementType = StandardElementType.Exposure,
        });

        new STXRulesProcessingService()
            .Evaluate(context: context)
            .Should()
            .NotContain(item => item.Code == "STX0004");
    }

    [Fact]
    public void HttpExposure_WhenDependingOnConcreteExposure_IsReported()
    {
        EvaluationContext context = CreateContext(
            elementType: StandardElementType.HttpExposure,
            dependencyType: StandardElementType.Exposure,
            dependencyName: "Example.Exposures.Templates.TemplateManager");

        context.ArchitectureElement.IsPublic = true;
        context.ArchitectureElement.AnalysisIsApiController = true;
        context.ArchitectureModel.Classes.Add(item: new Class
        {
            Name = "Example.Exposures.Templates.TemplateManager",
            IsPublic = true,
            Kind = ArchitectureTypeKind.Class,
            StandardElementType = StandardElementType.Exposure,
        });

        new STXRulesProcessingService()
            .Evaluate(context: context)
            .Should()
            .ContainSingle(item => item.Code == "STX0004");
    }

    [Fact]
    public void HttpExposure_WhenDependingOnReferencedPublicExposureContract_IsNotReported()
    {
        EvaluationContext context = CreateContext(
            elementType: StandardElementType.HttpExposure,
            dependencyType: StandardElementType.Exposure,
            dependencyName: "Example.Exposures.Templates.ITemplateManager");

        context.ArchitectureElement.IsPublic = true;
        context.ArchitectureElement.AnalysisDependencies.Single().IsInCurrentProject = false;
        context.ArchitectureElement.AnalysisDependencies.Single().IsPublicInterface = true;

        new STXRulesProcessingService()
            .Evaluate(context: context)
            .Should()
            .NotContain(item => item.Code == "STX0004");
    }

    [Fact]
    public void Exposure_WhenDependingOnOrchestrationService_IsNotReported()
    {
        EvaluationContext context = CreateContext(
            elementType: StandardElementType.HttpExposure,
            dependencyType: StandardElementType.OrchestrationService,
            dependencyName: "Example.Services.Orchestrations.Templates.ITemplateOrchestrationService");

        new STXRulesProcessingService()
            .Evaluate(context: context)
            .Should()
            .NotContain(item => item.Code == "STX0004");
    }

    [Fact]
    public void Dependency_WhenDependingOnDependency_IsReported()
    {
        EvaluationContext context = CreateContext(
            elementType: StandardElementType.Dependency,
            dependencyType: StandardElementType.Dependency,
            dependencyName: "Example.Dependencies.IInnerDependency");

        new STXRulesProcessingService()
            .Evaluate(context: context)
            .Should()
            .ContainSingle(item => item.Code == "STX0004");
    }

    [Fact]
    public void Exposure_WhenImplementingItsContract_IsNotReported()
    {
        EvaluationContext context = CreateContext(
            elementType: StandardElementType.Exposure,
            dependencyType: StandardElementType.OrchestrationService,
            dependencyName: "Example.Services.Orchestrations.Templates.ITemplateOrchestrationService");

        context.ArchitectureElement.AnalysisImplementedInterfaces =
            ["Example.Exposures.Templates.ITemplateManager"];

        new STXRulesProcessingService()
            .Evaluate(context: context)
            .Should()
            .NotContain(item => item.Code == "STX0004");
    }

    private static EvaluationContext CreateContext(
        StandardElementType elementType,
        StandardElementType dependencyType,
        string dependencyName)
    {
        Class element = new()
        {
            Name = "Example.Controllers.TemplateController",
            StandardElementType = elementType,
            AnalysisDeclarations = [],
            AnalysisImplementedInterfaces = [],
            AnalysisDependencies =
            [
                new TypeDependency
                {
                    TypeName = dependencyName,
                    StandardElementType = dependencyType,
                },
            ],
            AnalysisPublicApiModelTypes = [],
        };

        return new EvaluationContext
        {
            ArchitectureElement = element,
            ArchitectureModel = new Architecture
            {
                Classes = [element],
            },
        };
    }
}