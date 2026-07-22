// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;

namespace cCoder.CodeAnalysis.Services.Orchestrations.Rules;

internal sealed class HigherLevelServicesRuleEvaluationOrchestrationService(
    IProcessingServiceCodeAnalysisRulesProcessingService processingServiceRules,
    IOrchestrationServiceCodeAnalysisRulesProcessingService orchestrationServiceRules,
    ICoordinationServiceCodeAnalysisRulesProcessingService coordinationServiceRules,
    IManagementServiceCodeAnalysisRulesProcessingService managementServiceRules,
    IAggregationServiceCodeAnalysisRulesProcessingService aggregationServiceRules
) : IHigherLevelServicesRuleEvaluationOrchestrationService
{
    public AnalysisItem[] Evaluate(EvaluationContext context)
    {
        return context.StandardElementType switch
        {
            StandardElementType.ProcessingService => processingServiceRules.Evaluate(context),
            StandardElementType.OrchestrationService => orchestrationServiceRules.Evaluate(context),
            StandardElementType.CoordinationService => coordinationServiceRules.Evaluate(context),
            StandardElementType.ManagementService => managementServiceRules.Evaluate(context),
            StandardElementType.AggregationService => aggregationServiceRules.Evaluate(context),
            _ => Array.Empty<AnalysisItem>(),
        };
    }
}