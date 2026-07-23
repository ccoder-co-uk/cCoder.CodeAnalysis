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
        return contexts
            .SelectMany(Evaluate)
            .OrderBy<AnalysisItem, string>((AnalysisItem item) => item.Type, StringComparer.Ordinal)
            .ThenBy((AnalysisItem item) => item.LineNumber)
            .ThenBy<AnalysisItem, string>((AnalysisItem item) => item.Code, StringComparer.Ordinal)
            .ToArray();
    }

    private AnalysisItem[] Evaluate(EvaluationContext context)
    {
        return context.StandardElementType switch
        {
            StandardElementType.FoundationService or StandardElementType.Broker =>
                culDeSacRules.Evaluate(context),
            StandardElementType.Dependency => [],
            StandardElementType.ProcessingService
            or StandardElementType.OrchestrationService
            or StandardElementType.CoordinationService
            or StandardElementType.ManagementService
            or StandardElementType.AggregationService => higherLevelRules.Evaluate(context),
            StandardElementType.Exposure or StandardElementType.Model or StandardElementType.Test =>
                exposuresAndModelsRules.Evaluate(context),
            _ => RuleEvaluationCoordinationService.CreateInvalidElementTypeItem(context),
        };
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
