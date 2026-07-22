// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Coordinations.Rules;

internal interface IRuleEvaluationCoordinationService
{
    IReadOnlyList<AnalysisItem> Evaluate(IEnumerable<EvaluationContext> contexts);
}