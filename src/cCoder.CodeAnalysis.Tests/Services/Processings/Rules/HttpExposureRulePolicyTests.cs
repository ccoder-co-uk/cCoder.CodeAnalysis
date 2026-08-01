// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class HttpExposureRulePolicyTests
{
    [Fact]
    public void ApiDependencyCountShouldIgnoreHttpExposures()
    {
        EvaluationContext context = CreateContext(
            StandardElementType.HttpExposure,
            [
                CreateDependency(StandardElementType.FoundationService),
                CreateDependency(StandardElementType.HttpExposure),
            ],
            isApiController: true);

        new STXAPIRulesProcessingService().Evaluate(context)
            .Should().NotContain(item => item.Code == "STXAPI001");
    }

    [Fact]
    public void ExposureServiceCountShouldIgnoreHttpExposures()
    {
        EvaluationContext context = CreateContext(
            StandardElementType.Exposure,
            [
                CreateDependency(StandardElementType.FoundationService),
                CreateDependency(StandardElementType.HttpExposure),
            ]);

        new STXERulesProcessingService().Evaluate(context)
            .Should().NotContain(item => item.Code == "STXE003");
    }

    [Fact]
    public void BrokerShouldRejectHttpExposureDependency()
    {
        EvaluationContext context = CreateContext(
            StandardElementType.Broker,
            [CreateDependency(StandardElementType.HttpExposure)]);

        new STXBRulesProcessingService().Evaluate(context)
            .Should().ContainSingle(item => item.Code == "STXB006");
    }

    [Fact]
    public void FoundationShouldRejectHttpExposureDependency()
    {
        EvaluationContext context = CreateContext(
            StandardElementType.FoundationService,
            [CreateDependency(StandardElementType.HttpExposure)]);

        new STXFRulesProcessingService().Evaluate(context)
            .Should().ContainSingle(item => item.Code == "STXF002");
    }

    [Fact]
    public void HttpExposureShouldUseGenericExposureRules()
    {
        EvaluationContext exposure = CreateContext(StandardElementType.Exposure, []);
        EvaluationContext httpExposure = CreateContext(StandardElementType.HttpExposure, []);
        STXRulesProcessingService service = new();

        string[] exposureCodes = service.Evaluate(exposure)
            .Select(item => item.Code)
            .ToArray();
        string[] httpExposureCodes = service.Evaluate(httpExposure)
            .Select(item => item.Code)
            .ToArray();

        httpExposureCodes.Should().Equal(exposureCodes);
    }

    private static EvaluationContext CreateContext(
        StandardElementType elementType,
        IReadOnlyList<TypeDependency> dependencies,
        bool isApiController = false)
    {
        Class architectureElement = new()
        {
            Name = "Example.PolicyElement",
            StandardElementType = elementType,
            Properties = [],
            Methods = [],
            AnalysisDependencies = dependencies,
            AnalysisImplementedInterfaces = [],
            AnalysisIsApiController = isApiController,
            AnalysisTypeFacts = new TypeAnalysisFacts(),
        };

        return new EvaluationContext
        {
            TypeName = architectureElement.Name,
            StandardElementType = elementType,
            ArchitectureElement = architectureElement,
            Declarations = [],
            Dependencies = dependencies,
            ImplementedInterfaces = [],
            LocalDependencyTypeNames = [],
            PublicMethodNames = [],
            ContractMethodNames = [],
            PublicMethodCallLineNumbers = [],
            PublicApiModelTypes = [],
            ProjectTypeNames = [architectureElement.Name],
            UsingNamespaces = [],
        };
    }

    private static TypeDependency CreateDependency(StandardElementType elementType) =>
        new()
        {
            TypeName = $"Example.{elementType}",
            StandardElementType = elementType,
        };
}
