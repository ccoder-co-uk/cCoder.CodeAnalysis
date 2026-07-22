// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;

namespace cCoder.CodeAnalysis.Services.Orchestrations.Rules;

internal sealed class CulDeSacServicesAndBrokerRuleEvaluationOrchestrationService(
    IBrokerCodeAnalysisRulesProcessingService brokerRules,
    IFoundationServiceCodeAnalysisRulesProcessingService foundationServiceRules,
    IDependencyCodeAnalysisRulesProcessingService dependencyRules
) : ICulDeSacServicesAndBrokerRuleEvaluationOrchestrationService
{
    public AnalysisItem[] Evaluate(EvaluationContext context)
    {
        return context.StandardElementType switch
        {
            StandardElementType.Broker => brokerRules.Evaluate(context),
            StandardElementType.FoundationService => foundationServiceRules.Evaluate(context),
            StandardElementType.Dependency => dependencyRules.Evaluate(context),
            _ => Array.Empty<AnalysisItem>(),
        };
    }
}