// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class ServiceArchitectureModelRuleParityTests
{
    [Fact]
    public void AggregationDependencyRuleShouldUseAttachedModelFacts()
    {
        EvaluationContext context = CreateContext(
            typeName: "StudentAggregationService",
            modelDependencies:
            [
                CreateDependency(StandardElementType.FoundationService),
                CreateDependency(StandardElementType.FoundationService),
            ]);

        new STXARulesProcessingService().Evaluate(context: context)
            .Should().NotContain(item => item.Code == "STXA001");
    }

    [Fact]
    public void CoordinationDependencyRuleShouldUseAttachedModelFacts()
    {
        EvaluationContext context = CreateContext(
            typeName: "StudentCoordinationService",
            modelDependencies:
            [
                CreateDependency(StandardElementType.OrchestrationService),
                CreateDependency(StandardElementType.OrchestrationService),
            ]);

        new STXCRulesProcessingService().Evaluate(context: context)
            .Should().NotContain(item => item.Code == "STXC001");
    }

    [Fact]
    public void FoundationDependencyRuleShouldUseAttachedModelFacts()
    {
        EvaluationContext context = CreateContext(
            typeName: "StudentService",
            modelDependencies: [CreateDependency(StandardElementType.Broker)]);

        new STXFRulesProcessingService().Evaluate(context: context)
            .Should().NotContain(item => item.Code == "STXF002");
    }

    [Fact]
    public void FoundationConcurrencyRuleShouldUseAttachedModelFacts()
    {
        EvaluationContext context = CreateContext(
            typeName: "StudentService",
            modelDependencies: [CreateDependency(StandardElementType.Broker)]);
        context.ArchitectureElement.Methods =
        [
            new Method
            {
                Name = "UpdateStudentAsync",
                LineNumber = 21,
                IncomingExceptionTypes =
                    ["Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException"],
                ThrowsExceptionTypes =
                    ["Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException"],
                DirectCalls = [],
            },
        ];

        new STXFRulesProcessingService().Evaluate(context: context)
            .Should().ContainSingle(item => item.Code == "STXF004");
    }

    [Fact]
    public void FoundationConcurrencyRuleShouldAcceptWrappedException()
    {
        EvaluationContext context = CreateContext(
            typeName: "StudentService",
            modelDependencies: [CreateDependency(StandardElementType.Broker)]);
        context.ArchitectureElement.Methods =
        [
            new Method
            {
                Name = "UpdateStudentAsync",
                IncomingExceptionTypes =
                    ["Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException"],
                ThrowsExceptionTypes = ["StudentServiceConcurrencyException"],
                DirectCalls = [],
            },
        ];

        new STXFRulesProcessingService().Evaluate(context: context)
            .Should().NotContain(item => item.Code == "STXF004");
    }

    [Fact]
    public void OrchestrationDependencyRuleShouldUseAttachedModelFacts()
    {
        EvaluationContext context = CreateContext(
            typeName: "StudentOrchestrationService",
            modelDependencies:
            [
                CreateDependency(StandardElementType.FoundationService),
                CreateDependency(StandardElementType.FoundationService),
            ]);

        new STXORulesProcessingService().Evaluate(context: context)
            .Should().NotContain(item => item.Code == "STXO001");
    }

    [Fact]
    public void ParentDependencyCountShouldIgnoreUtilityBrokerAndConfiguration()
    {
        EvaluationContext context = CreateContext(
            typeName: "StudentOrchestrationService",
            modelDependencies:
            [
                CreateDependency(StandardElementType.FoundationService),
                CreateDependency(StandardElementType.FoundationService),
                new TypeDependency
                {
                    TypeName = "ILoggingBroker",
                    StandardElementType = StandardElementType.Broker,
                },
                new TypeDependency
                {
                    TypeName = "StudentConfiguration",
                    StandardElementType = StandardElementType.Model,
                    IsConfigurationModel = true,
                },
            ]);

        new STXORulesProcessingService().Evaluate(context: context)
            .Should().NotContain(item => item.Code == "STXO001");
    }

    [Fact]
    public void ProcessingDependencyRulesShouldUseAttachedModelFacts()
    {
        EvaluationContext context = CreateContext(
            typeName: "StudentProcessingService",
            modelDependencies:
            [
                CreateDependency(
                    standardElementType: StandardElementType.FoundationService,
                    typeName: "IStudentService"),
            ]);

        new STXPRulesProcessingService().Evaluate(context: context)
            .Should().NotContain(item => item.Code == "STXP001" || item.Code == "STXP003");
    }

    [Fact]
    public void ManagementDependencyRuleShouldUseAttachedModelFacts()
    {
        EvaluationContext context = CreateContext(
            typeName: "StudentManagementService",
            modelDependencies:
            [
                CreateDependency(StandardElementType.CoordinationService),
                CreateDependency(StandardElementType.CoordinationService),
            ]);

        new STXMGRulesProcessingService().Evaluate(context: context)
            .Should().NotContain(item => item.Code == "STXMG001");
    }

    private static EvaluationContext CreateContext(
        string typeName,
        IReadOnlyList<TypeDependency> modelDependencies)
    {
        Class architectureElement = new()
        {
            Name = typeName,
            AnalysisDependencies = modelDependencies,
            AnalysisDeclarations = [],
            AnalysisImplementedInterfaces = [],
            AnalysisPublicApiModelTypes = [],
        };

        return new EvaluationContext
        {
            ArchitectureElement = architectureElement,
            ArchitectureModel = new Architecture
            {
                Classes = [architectureElement],
            },
        };
    }

    private static TypeDependency CreateDependency(
        StandardElementType standardElementType,
        string typeName = "IDependency") =>
        new()
        {
            StandardElementType = standardElementType,
            TypeName = typeName,
        };
}
