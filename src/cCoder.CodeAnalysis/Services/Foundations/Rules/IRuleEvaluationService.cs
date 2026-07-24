// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Foundations.Rules;

internal interface IRuleEvaluationService : ICodeAnalysisInfrastructureService
{
    IEnumerable<AnalysisItem> Evaluate(EvaluationContext context);
}