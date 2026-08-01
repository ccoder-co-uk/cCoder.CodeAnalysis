// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class STXDRulesProcessingServiceTests
{
    [Fact]
    public void EvaluateShouldRejectExternalResourceInBroker()
    {
        Class architectureElement = new()
        {
            Name = "Example.Brokers.MailBroker",
            StandardElementType = StandardElementType.Broker,
            AnalysisDeclarations = [],
            AnalysisDependencies = [],
            AnalysisImplementedInterfaces = ["Example.Brokers.IMailBroker"],
            AnalysisUsesExternalResource = true,
        };
        EvaluationContext context = CreateContext(architectureElement);

        STXDRulesProcessingService service = new();

        service.Evaluate(context: context)
            .Should()
            .ContainSingle(
                predicate: item => item.Code == "STXD004");
    }

    [Fact]
    public void EvaluateShouldRejectExternalResourceInHttpExposure()
    {
        Class architectureElement = new()
        {
            Name = "Example.Controllers.StudentController",
            StandardElementType = StandardElementType.HttpExposure,
            AnalysisDeclarations = [],
            AnalysisDependencies = [],
            AnalysisImplementedInterfaces = [],
            AnalysisUsesExternalResource = true,
        };
        EvaluationContext context = CreateContext(architectureElement);

        new STXDRulesProcessingService().Evaluate(context)
            .Should().ContainSingle(item => item.Code == "STXD004");
    }

    private static EvaluationContext CreateContext(Class architectureElement) =>
        new()
        {
            ArchitectureElement = architectureElement,
            ArchitectureModel = new Architecture
            {
                Classes = [architectureElement],
                AnalysisLocalDependencyTypeNames = [],
            },
        };
}
