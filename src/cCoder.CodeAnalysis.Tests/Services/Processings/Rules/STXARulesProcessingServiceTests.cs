// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class STXARulesProcessingServiceTests
{
    [Fact]
    public void AggregationService_WhenOnlyOneBusinessServiceIsRequired_IsReported()
    {
        // given
        ClassDeclarationSyntax declaration = CSharpSyntaxTree
            .ParseText(text:
                """
                internal sealed class StudentAggregationService
                {
                    public ValueTask AddAsync(Student student) =>
                        studentOrchestrationService.AddAsync(student);
                }
                """)
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single();

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
            ],
            AnalysisDeclarations = [declaration]
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
    public void AggregationService_WhenOneBusinessServiceAndBusinessLogicAreRequired_IsNotReported()
    {
        // given
        ClassDeclarationSyntax declaration = CSharpSyntaxTree
            .ParseText(text:
                """
                internal sealed class StudentAggregationService
                {
                    public int Add(int value)
                    {
                        if (value < 0)
                        {
                            throw new ArgumentException();
                        }

                        return value + 1;
                    }
                }
                """)
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single();

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
            ],
            AnalysisDeclarations = [declaration]
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
    public void AggregationService_WhenTwoCompositionExposuresAreRequired_IsNotReported()
    {
        // given
        Class firstCompositionExposureContract = new()
        {
            Name = "Example.Exposures.IFirstCompositionExposure",
            StandardElementType = StandardElementType.Exposure,
            Kind = ArchitectureTypeKind.Interface
        };

        Class secondCompositionExposureContract = new()
        {
            Name = "Example.Exposures.ISecondCompositionExposure",
            StandardElementType = StandardElementType.Exposure,
            Kind = ArchitectureTypeKind.Interface
        };

        Class firstCompositionExposure = new()
        {
            Name = "Example.Exposures.FirstCompositionExposure",
            StandardElementType = StandardElementType.Exposure,
            Interfaces =
            [
                new TypeReference
                {
                    FullName = firstCompositionExposureContract.Name,
                    Name = "IFirstCompositionExposure"
                }
            ],
            AnalysisDirectlyImplementedInterfaces =
            [
                "cCoder.CodeAnalysis.Exposures.ICompositionExposure"
            ]
        };

        Class secondCompositionExposure = new()
        {
            Name = "Example.Exposures.SecondCompositionExposure",
            StandardElementType = StandardElementType.Exposure,
            Interfaces =
            [
                new TypeReference
                {
                    FullName = secondCompositionExposureContract.Name,
                    Name = "ISecondCompositionExposure"
                }
            ],
            AnalysisDirectlyImplementedInterfaces =
            [
                "cCoder.CodeAnalysis.Exposures.ICompositionExposure"
            ]
        };

        Class architectureElement = new()
        {
            Name = "Example.Services.Aggregations.StudentAggregationService",
            StandardElementType = StandardElementType.AggregationService,
            AnalysisDependencies =
            [
                new TypeDependency
                {
                    TypeName = firstCompositionExposureContract.Name,
                    StandardElementType = StandardElementType.Exposure
                },
                new TypeDependency
                {
                    TypeName = secondCompositionExposureContract.Name,
                    StandardElementType = StandardElementType.Exposure
                }
            ]
        };

        EvaluationContext context = new()
        {
            ArchitectureElement = architectureElement,
            ArchitectureModel = new Architecture
            {
                Classes =
                [
                    architectureElement,
                    firstCompositionExposure,
                    secondCompositionExposure
                ],
                Interfaces =
                [
                    firstCompositionExposureContract,
                    secondCompositionExposureContract
                ]
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