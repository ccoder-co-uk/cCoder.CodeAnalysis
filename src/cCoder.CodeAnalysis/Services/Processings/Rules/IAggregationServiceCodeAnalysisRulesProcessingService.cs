// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal interface IAggregationServiceCodeAnalysisRulesProcessingService
{
    AnalysisItem[] Evaluate(EvaluationContext context);
}