// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Orchestrations.Rules;

namespace cCoder.CodeAnalysis.Services.Coordinations.Rules;

internal sealed class RuleEvaluationCoordinationService(
    ICulDeSacServicesAndBrokerRuleEvaluationOrchestrationService culDeSacRules,
    IHigherLevelServicesRuleEvaluationOrchestrationService higherLevelRules,
    IExposuresAndModelsRuleEvaluationOrchestrationService exposuresAndModelsRules
) : IRuleEvaluationCoordinationService
{
    public IReadOnlyList<AnalysisItem> Evaluate(IEnumerable<EvaluationContext> contexts)
    {
        EvaluationContext[] contextArray = contexts.ToArray();
        HashSet<string> localDependencyTypeNames = new HashSet<string>(
            contextArray
                .Where(
                    (EvaluationContext context) =>
                        context.StandardElementType == StandardElementType.Dependency
                )
                .Select((EvaluationContext context) => context.TypeName),
            StringComparer.Ordinal
        );

        return contextArray
            .SelectMany(
                (EvaluationContext context) =>
                    Evaluate(context, localDependencyTypeNames)
            )
            .OrderBy<AnalysisItem, string>((AnalysisItem item) => item.Type, StringComparer.Ordinal)
            .ThenBy((AnalysisItem item) => item.LineNumber)
            .ThenBy<AnalysisItem, string>((AnalysisItem item) => item.Code, StringComparer.Ordinal)
            .ToArray();
    }

    private AnalysisItem[] Evaluate(
        EvaluationContext context,
        HashSet<string> localDependencyTypeNames
    )
    {
        AnalysisItem[] elementItems = context.StandardElementType switch
        {
            StandardElementType.FoundationService or StandardElementType.Broker =>
                culDeSacRules.Evaluate(context),
            StandardElementType.Dependency => [],
            StandardElementType.ProcessingService
            or StandardElementType.OrchestrationService
            or StandardElementType.CoordinationService
            or StandardElementType.ManagementService
            or StandardElementType.AggregationService => higherLevelRules.Evaluate(context),
            StandardElementType.App or StandardElementType.Exposure or StandardElementType.Model or StandardElementType.Test =>
                exposuresAndModelsRules.Evaluate(context),
            _ => RuleEvaluationCoordinationService.CreateInvalidElementTypeItem(context),
        };

        AnalysisItem[] dependencyItems =
            RuleEvaluationCoordinationService.EvaluateDependencyConsumption(
                context,
                localDependencyTypeNames
            );

        return elementItems.Concat(dependencyItems).ToArray();
    }

    private static AnalysisItem[] EvaluateDependencyConsumption(
        EvaluationContext context,
        HashSet<string> localDependencyTypeNames
    )
    {
        bool consumesDependency = context.Dependencies.Any(
            (TypeDependency dependency) =>
                dependency.StandardElementType == StandardElementType.Dependency
                && localDependencyTypeNames.Contains(dependency.TypeName)
        );

        return context.StandardElementType == StandardElementType.Broker || !consumesDependency
            ? []
            :
            [
                new AnalysisItem
                {
                    Code = "STXD001",
                    Description = "Dependency elements may only be consumed by brokers.",
                    Severity = AnalysisSeverity.Warning,
                    Type = context.TypeName,
                    LineNumber = context.LineNumber,
                },
            ];
    }

    private static AnalysisItem[] CreateInvalidElementTypeItem(EvaluationContext context)
    {
        return
        [
            new AnalysisItem
            {
                Code = "STX0001",
                Description = "The type is not a valid Standard element type.",
                Severity = AnalysisSeverity.Warning,
                Type = context.TypeName,
                LineNumber = context.LineNumber,
            },
        ];
    }
}
