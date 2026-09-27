// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class STXARulesProcessingServiceTests
{
    [Fact]
    public void AggregationService_WhenOnlyOneBusinessServiceIsRequired_IsReported()
    {
        // given
        Class architectureElement = new()
        {
            Name = "Example.Services.Aggregations.StudentAggregationService",
            StandardElementType = StandardElementType.AggregationService,
            AnalysisDependencies =
            [
                new TypeDependency
                {
                    StandardElementType =
                        StandardElementType.OrchestrationService
                }
            ]
        };
        EvaluationContext context = new()
        {
            ArchitectureElement = architectureElement,
            ArchitectureModel = new Architecture
            {
                Classes = [architectureElement],
            },
        };

        STXARulesProcessingService service = new();

        // when
        AnalysisItem[] results = service
            .Evaluate(context: context)
            .ToArray();

        // then
        results
            .Should()
            .ContainSingle(
                predicate: result => result.Code == "STXA003");
    }

    [Fact]
    public void AggregationService_WhenTwoBusinessServicesAreRequired_IsNotReported()
    {
        // given
        Class architectureElement = new()
        {
            Name = "Example.Services.Aggregations.StudentAggregationService",
            StandardElementType = StandardElementType.AggregationService,
            AnalysisDependencies =
            [
                new TypeDependency
                {
                    StandardElementType =
                        StandardElementType.OrchestrationService
                },
                new TypeDependency
                {
                    StandardElementType =
                        StandardElementType.OrchestrationService
                }
            ]
        };
        EvaluationContext context = new()
        {
            ArchitectureElement = architectureElement,
            ArchitectureModel = new Architecture
            {
                Classes = [architectureElement],
            },
        };

        STXARulesProcessingService service = new();

        // when
        AnalysisItem[] results = service
            .Evaluate(context: context)
            .ToArray();

        // then
        results
            .Should()
            .NotContain(
                predicate: result => result.Code == "STXA003");
    }

    [Fact]
    public void EvaluateShouldIgnoreNonServiceDependencies()
    {
        // given
        Class architectureElement = new()
        {
            Name = "Example.AggregationService",
            StandardElementType = StandardElementType.AggregationService,
            AnalysisDependencies =
            [
                new TypeDependency
                {
                    StandardElementType =
                        StandardElementType.FoundationService
                },
                new TypeDependency
                {
                    StandardElementType = StandardElementType.Model
                },
                new TypeDependency
                {
                    StandardElementType = StandardElementType.Dependency
                }
            ]
        };
        EvaluationContext context = new()
        {
            ArchitectureElement = architectureElement,
            ArchitectureModel = new Architecture
            {
                Classes = [architectureElement],
            },
        };

        STXARulesProcessingService service = new();

        // when
        AnalysisItem[] results = service
            .Evaluate(context: context)
            .ToArray();

        // then
        results
            .Should()
            .NotContain(
                predicate: result => result.Code == "STXA001");
    }
}