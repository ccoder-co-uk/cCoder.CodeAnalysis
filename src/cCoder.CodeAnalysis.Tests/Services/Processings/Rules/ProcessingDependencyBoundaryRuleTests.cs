// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class ProcessingDependencyBoundaryRuleTests
{
    [Fact]
    public void ProcessingService_WhenDependingDirectlyOnBroker_IsReported()
    {
        EvaluationContext context = CreateContext(
            CreateDependency(
                standardElementType: StandardElementType.FoundationService,
                typeName: "IStudentService"),
            CreateDependency(
                standardElementType: StandardElementType.Broker,
                typeName: "IStudentBroker"));

        new STXPRulesProcessingService().Evaluate(context: context)
            .Should().ContainSingle(item => item.Code == "STXP001");
    }

    [Fact]
    public void ProcessingService_WhenDependingDirectlyOnExposure_IsReported()
    {
        EvaluationContext context = CreateContext(
            CreateDependency(
                standardElementType: StandardElementType.FoundationService,
                typeName: "IStudentService"),
            CreateDependency(
                standardElementType: StandardElementType.Exposure,
                typeName: "IStudentManager"));

        new STXPRulesProcessingService().Evaluate(context: context)
            .Should().ContainSingle(item => item.Code == "STXP001");
    }

    [Fact]
    public void ProcessingService_WhenDependingOnOneMatchingFoundationAndBclTypes_IsAllowed()
    {
        EvaluationContext context = CreateContext(
            CreateDependency(
                standardElementType: StandardElementType.FoundationService,
                typeName: "IStudentService"),
            CreateDependency(
                standardElementType: StandardElementType.Unknown,
                typeName: "System.String"),
            CreateDependency(
                standardElementType: StandardElementType.Unknown,
                typeName: "System.Threading.CancellationToken"));

        new STXPRulesProcessingService().Evaluate(context: context)
            .Should().NotContain(item => item.Code == "STXP001" || item.Code == "STXP003");
    }

    private static EvaluationContext CreateContext(params TypeDependency[] dependencies)
    {
        Class processingService = new()
        {
            Name = "StudentProcessingService",
            StandardElementType = StandardElementType.ProcessingService,
            AnalysisDependencies = dependencies,
            AnalysisDeclarations = [],
            AnalysisImplementedInterfaces = [],
            AnalysisPublicApiModelTypes = [],
        };

        return new EvaluationContext
        {
            ArchitectureElement = processingService,
            ArchitectureModel = new Architecture
            {
                Classes = [processingService],
            },
        };
    }

    private static TypeDependency CreateDependency(
        StandardElementType standardElementType,
        string typeName) =>
        new()
        {
            StandardElementType = standardElementType,
            TypeName = typeName,
        };
}
