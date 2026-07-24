// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Foundations.Rules;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class RuleEvaluationsProcessingService(IRuleEvaluationService ruleEvaluationService)
    : IRuleEvaluationsProcessingService
{
    public IReadOnlyList<AnalysisItem> Process(IEnumerable<EvaluationContext> contexts)
    {
        EvaluationContext[] evaluationContexts = contexts.ToArray();

        return Evaluate(contexts: evaluationContexts)
            .OrderBy<AnalysisItem, string>(
                keySelector: (AnalysisItem item) => item.Type,
                comparer: StringComparer.Ordinal
            )
            .ThenBy(keySelector: (AnalysisItem item) => item.LineNumber)
            .ThenBy<AnalysisItem, string>(
                keySelector: (AnalysisItem item) => item.Code,
                comparer: StringComparer.Ordinal
            )
            .ToArray();
    }

    private IEnumerable<AnalysisItem> Evaluate(IEnumerable<EvaluationContext> contexts)
    {
        foreach (EvaluationContext context in contexts)
        {
            foreach (AnalysisItem item in ruleEvaluationService.Evaluate(context: context))
            {
                yield return item;
            }
        }
    }
}