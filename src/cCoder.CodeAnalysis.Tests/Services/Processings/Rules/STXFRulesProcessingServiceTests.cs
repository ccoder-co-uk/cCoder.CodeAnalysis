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
        EvaluationContext context = new()
        {
            TypeName = "Example.Services.Foundations.StudentService",
            StandardElementType = StandardElementType.FoundationService,
            Declarations = [],
            Dependencies =
            [
                new TypeDependency
                {
                    TypeName =
                        "Microsoft.Extensions.Logging.ILogger<Example.Services.Foundations.StudentService>",
                    StandardElementType = StandardElementType.Dependency
                }
            ]
        };

        STXFRulesProcessingService service = new();

        AnalysisItem[] results = service
            .Evaluate(context: context)
            .ToArray();

        results
            .Should()
            .NotContain(predicate: result => result.Code == "STXF002");
    }
}
