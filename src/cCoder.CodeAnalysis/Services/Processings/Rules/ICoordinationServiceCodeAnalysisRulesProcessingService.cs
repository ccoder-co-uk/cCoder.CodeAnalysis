// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal interface ICoordinationServiceCodeAnalysisRulesProcessingService
{
    AnalysisItem[] Evaluate(EvaluationContext context);
}