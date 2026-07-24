// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXCRulesProcessingService : ISTXCRulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        foreach (AnalysisItem item in EvaluateSTXC001(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXC002(context: context))
        {
            yield return item;
        }
    }

    private static AnalysisItem CreateAnalysisItem(
        string code,
        string description,
        EvaluationContext context,
        Microsoft.CodeAnalysis.Location? location = null
    )
    {
        return new AnalysisItem
        {
            Code = code,
            Description = description,
            Severity = AnalysisSeverity.Warning,
            Type = context.TypeName,
            LineNumber = location is null ? context.LineNumber : location.GetLineSpan().StartLinePosition.Line + 1,
        };
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXC001(EvaluationContext context)
    {
        int dependencyCount = context.Dependencies.Count;
        bool hasValidCount = dependencyCount is 2 or 3;

        bool containsOnlyOrchestrations = context.Dependencies.All(
            predicate: (TypeDependency dependency) =>
                dependency.StandardElementType == StandardElementType.OrchestrationService
        );

        return hasValidCount && containsOnlyOrchestrations
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXC001",
                    description: "The service must have two or three OrchestrationService dependencies.",
                    context: context
                ),
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXC002(EvaluationContext context)
    {
        string typeName = context.TypeName.Split(separator: ['.'])
            .Last();

        return typeName.Contains(value: "Coordination", comparisonType: StringComparison.Ordinal)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: "STXC002",
                    description: "A coordination service name must contain the Coordination identifier.",
                    context: context
                ),
            };
    }
}