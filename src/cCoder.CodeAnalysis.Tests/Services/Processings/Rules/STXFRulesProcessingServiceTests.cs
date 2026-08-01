// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class STXFRulesProcessingServiceTests
{
    [Fact]
    public void EvaluateShouldAllowLoggingInfrastructure()
    {
        Class architectureElement = new()
        {
            Name = "Example.Services.Foundations.StudentService",
            StandardElementType = StandardElementType.FoundationService,
            AnalysisDeclarations = [],
            AnalysisImplementedInterfaces = [],
            AnalysisDependencies =
            [
                new TypeDependency
                {
                    TypeName =
                        "Microsoft.Extensions.Logging.ILogger<Example.Services.Foundations.StudentService>",
                    StandardElementType = StandardElementType.Dependency
                }
            ]
        };
        EvaluationContext context = CreateContext(architectureElement);

        STXFRulesProcessingService service = new();

        AnalysisItem[] results = service
            .Evaluate(context: context)
            .ToArray();

        results
            .Should()
            .NotContain(predicate: result => result.Code == "STXF002");
    }

    private static EvaluationContext CreateContext(Class architectureElement) =>
        new()
        {
            ArchitectureElement = architectureElement,
            ArchitectureModel = new Architecture
            {
                Classes = [architectureElement],
            },
        };
}
