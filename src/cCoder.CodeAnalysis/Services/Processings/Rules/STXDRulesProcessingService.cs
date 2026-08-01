// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXDRulesProcessingService : ISTXDRulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateSTXD001(context: context)
            .Concat(second: EvaluateSTXD002(context: context))
            .Concat(second: EvaluateSTXD003(context: context))
            .Concat(second: EvaluateSTXD004(context: context));
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXD001(EvaluationContext context)
    {
        bool consumesDependency = context.Dependencies.Any(
            predicate: (TypeDependency dependency) =>
                dependency.StandardElementType == StandardElementType.Dependency
                && context.LocalDependencyTypeNames.Contains(value: dependency.TypeName)
        );

        bool mayConsumeDependency =
            context.StandardElementType == StandardElementType.Broker
            || context.StandardElementType == StandardElementType.Dependency
            || context.DeclaresDependencyIntent
            || IsHostedService(context: context);

        if (!mayConsumeDependency && consumesDependency)
        {
            yield return new AnalysisItem
            {
                Code = "STXD001",
                Description = "Dependency elements may only be consumed by brokers or other dependencies.",
                Severity = AnalysisSeverity.Warning,
                Type = context.TypeName,
                LineNumber = context.LineNumber,
            };
        }
    }

    private static bool IsHostedService(
        EvaluationContext context) =>
        context.ImplementedInterfaces?.Any(
            predicate: (string interfaceName) =>
                interfaceName.EndsWith(
                    value: ".IHostedService",
                    comparisonType: StringComparison.Ordinal)
                || interfaceName == "IHostedService")
            == true;

    private static IEnumerable<AnalysisItem> EvaluateSTXD002(EvaluationContext context)
    {
        if (
            context.DeclaresDependencyIntent
            && !context.HasExternalBaseType
            && !context.ImplementsExternalInterface
            && !context.ImplementsContract
            && !context.HasExternalStateDependency
        )
        {
            yield return new AnalysisItem
            {
                Code = "STXD002",
                Description =
                    "A dependency must inherit an external type or implement an external interface that the application cannot control.",
                Severity = AnalysisSeverity.Warning,
                Type = context.TypeName,
                LineNumber = context.LineNumber,
            };
        }
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXD003(
        EvaluationContext context)
    {
        if (context.DeclaresDependencyIntent
            && context.ExposesExternalResource)
        {
            yield return new AnalysisItem
            {
                Code = "STXD003",
                Description =
                    "A dependency must encapsulate external resources rather than expose them through its surface.",
                Severity = AnalysisSeverity.Warning,
                Type = context.TypeName,
                LineNumber = context.LineNumber,
            };
        }
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXD004(
        EvaluationContext context)
    {
        bool isBusinessElement = IsBusinessElement(context.StandardElementType);

        if (isBusinessElement
            && context.UsesExternalResource)
        {
            yield return new AnalysisItem
            {
                Code = "STXD004",
                Description =
                    "Externally controlled operational resources must be owned by a dependency.",
                Severity = AnalysisSeverity.Warning,
                Type = context.TypeName,
                LineNumber = context.LineNumber,
            };
        }
    }

    private static bool IsBusinessElement(StandardElementType elementType) =>
        elementType is StandardElementType.Broker
            or StandardElementType.Exposure
            or StandardElementType.HttpExposure
            or StandardElementType.FoundationService
            or StandardElementType.ProcessingService
            or StandardElementType.OrchestrationService
            or StandardElementType.CoordinationService
            or StandardElementType.ManagementService
            or StandardElementType.AggregationService;
}
