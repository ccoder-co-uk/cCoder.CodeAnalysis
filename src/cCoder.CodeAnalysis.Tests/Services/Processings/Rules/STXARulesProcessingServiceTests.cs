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
    public void EvaluateShouldIgnoreNonServiceDependencies()
    {
        // given
        EvaluationContext context = new()
        {
            TypeName = "Example.AggregationService",
            StandardElementType = StandardElementType.AggregationService,
            Dependencies =
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
