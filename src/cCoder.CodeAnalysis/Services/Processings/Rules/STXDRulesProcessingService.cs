// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXDRulesProcessingService : ISTXDRulesProcessingService
{
    private static readonly IArchitectureModelQueriesProcessingService
        architectureModelQueries = new ArchitectureModelQueriesProcessingService();

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateSTXD001(context: context)
            .Concat(second: EvaluateSTXD002(context: context))
            .Concat(second: EvaluateSTXD003(context: context))
            .Concat(second: EvaluateSTXD004(context: context));
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXD001(EvaluationContext context)
    {
        bool consumesDependency = architectureModelQueries.GetDependencies(context: context).Any(
            predicate: (TypeDependency dependency) =>
                dependency.StandardElementType == StandardElementType.Dependency
                && architectureModelQueries.GetLocalDependencyTypeNames(context: context).Contains(value: dependency.TypeName)
        );

        bool mayConsumeDependency =
            architectureModelQueries.GetStandardElementType(context: context) == StandardElementType.Broker
            || architectureModelQueries.GetStandardElementType(context: context) == StandardElementType.Dependency
            || architectureModelQueries.DeclaresDependencyIntent(context: context)
            || IsHostedService(context: context);

        if (!mayConsumeDependency && consumesDependency)
        {
            yield return new AnalysisItem
            {
                Code = "STXD001",
                Description = "Dependency elements may only be consumed by brokers or other dependencies.",
                Severity = AnalysisSeverity.Warning,
                Type = architectureModelQueries.GetTypeName(context: context),
                LineNumber = architectureModelQueries.GetLineNumber(context: context),
            };
        }
    }

    private static bool IsHostedService(
        EvaluationContext context) =>
        architectureModelQueries.GetImplementedInterfaces(context: context)?.Any(
            predicate: (string interfaceName) =>
                interfaceName.EndsWith(
                    value: ".IHostedService",
                    comparisonType: StringComparison.Ordinal)
                || interfaceName == "IHostedService")
            == true;

    private static IEnumerable<AnalysisItem> EvaluateSTXD002(EvaluationContext context)
    {
        if (
            architectureModelQueries.DeclaresDependencyIntent(context: context)
            && !architectureModelQueries.HasExternalBaseType(context: context)
            && !architectureModelQueries.ImplementsExternalInterface(context: context)
            && !architectureModelQueries.ImplementsContract(context: context)
            && !architectureModelQueries.HasExternalStateDependency(context: context)
        )
        {
            yield return new AnalysisItem
            {
                Code = "STXD002",
                Description =
                    "A dependency must inherit an external type or implement an external interface that the application cannot control.",
                Severity = AnalysisSeverity.Warning,
                Type = architectureModelQueries.GetTypeName(context: context),
                LineNumber = architectureModelQueries.GetLineNumber(context: context),
            };
        }
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXD003(
        EvaluationContext context)
    {
        if (architectureModelQueries.DeclaresDependencyIntent(context: context)
            && architectureModelQueries.ExposesExternalResource(context: context))
        {
            yield return new AnalysisItem
            {
                Code = "STXD003",
                Description =
                    "A dependency must encapsulate external resources rather than expose them through its surface.",
                Severity = AnalysisSeverity.Warning,
                Type = architectureModelQueries.GetTypeName(context: context),
                LineNumber = architectureModelQueries.GetLineNumber(context: context),
            };
        }
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXD004(
        EvaluationContext context)
    {
        bool isBusinessElement = IsBusinessElement(architectureModelQueries.GetStandardElementType(context: context));

        if (isBusinessElement
            && architectureModelQueries.UsesExternalResource(context: context))
        {
            yield return new AnalysisItem
            {
                Code = "STXD004",
                Description =
                    "Externally controlled operational resources must be owned by a dependency.",
                Severity = AnalysisSeverity.Warning,
                Type = architectureModelQueries.GetTypeName(context: context),
                LineNumber = architectureModelQueries.GetLineNumber(context: context),
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
