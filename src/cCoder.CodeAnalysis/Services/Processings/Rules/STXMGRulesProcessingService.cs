// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXMGRulesProcessingService : ISTXMGRulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        foreach (AnalysisItem item in EvaluateSTXMG001(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXMG002(context: context))
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

    private static IEnumerable<AnalysisItem> EvaluateSTXMG001(EvaluationContext context)
    {
        int dependencyCount = context.Dependencies.Count;
        bool hasValidCount = dependencyCount is 2 or 3;

        bool containsOnlyCoordinations = context.Dependencies.All(
            predicate: (TypeDependency dependency) =>
                dependency.StandardElementType == StandardElementType.CoordinationService
        );

        return hasValidCount && containsOnlyCoordinations
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXMG001",
                    description: "The service must have two or three CoordinationService dependencies.",
                    context: context
                ),
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXMG002(EvaluationContext context)
    {
        string typeName = context.TypeName.Split(separator: ['.'])
            .Last();

        return typeName.Contains(value: "Management", comparisonType: StringComparison.Ordinal)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: "STXMG002",
                    description: "A management service name must contain the Management identifier.",
                    context: context
                ),
            };
    }
}