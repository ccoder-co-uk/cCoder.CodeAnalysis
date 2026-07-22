// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;

namespace cCoder.CodeAnalysis.Services.Orchestrations.Rules;

internal sealed class ExposuresAndModelsRuleEvaluationOrchestrationService(
    IExposureCodeAnalysisRulesProcessingService exposureRules,
    IModelCodeAnalysisRulesProcessingService modelRules,
    ITestCodeAnalysisRulesProcessingService testRules
) : IExposuresAndModelsRuleEvaluationOrchestrationService
{
    public AnalysisItem[] Evaluate(EvaluationContext context)
    {
        return context.StandardElementType switch
        {
            StandardElementType.Exposure => exposureRules.Evaluate(context),
            StandardElementType.Model => modelRules.Evaluate(context),
            StandardElementType.Test => testRules.Evaluate(context),
            _ => Array.Empty<AnalysisItem>(),
        };
    }
}