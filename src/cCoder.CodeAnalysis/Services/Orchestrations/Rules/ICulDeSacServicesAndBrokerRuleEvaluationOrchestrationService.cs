// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Orchestrations.Rules;

internal interface ICulDeSacServicesAndBrokerRuleEvaluationOrchestrationService
{
    AnalysisItem[] Evaluate(EvaluationContext context);
}