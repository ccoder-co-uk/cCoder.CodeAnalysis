// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXORulesProcessingService : ISTXORulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        foreach (AnalysisItem item in EvaluateSTXO001(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXO002(context: context))
        {
            yield return item;
        }
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXO001(EvaluationContext context)
    {
        int count = context.Dependencies.Count;
        bool flag = (uint)(count - 2) <= 1u;
        bool hasValidCount = flag;

        bool containsFoundation = context.Dependencies.Any(
            predicate: (TypeDependency dependency) =>
                dependency.StandardElementType == StandardElementType.FoundationService
        );

        bool containsProcessing = context.Dependencies.Any(
            predicate: (TypeDependency dependency) =>
                dependency.StandardElementType == StandardElementType.ProcessingService
        );

        bool containsOnlySupportedDependencies = context.Dependencies.All(
            predicate: delegate(TypeDependency dependency)
            {
                StandardElementType standardElementType = dependency.StandardElementType;
                return (uint)(standardElementType - 2) <= 1u;
            }
        );

        bool mixesDependencyTypes = containsFoundation && containsProcessing;

        return (hasValidCount && containsOnlySupportedDependencies && !mixesDependencyTypes)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                new AnalysisItem
                {
                    Code = "STXO001",
                    Description =
                        "An orchestration must have two or three foundation or processing dependencies and must not mix them.",
                    Severity = AnalysisSeverity.Warning,
                    Type = context.TypeName,
                    LineNumber = context.LineNumber,
                },
            };
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXO002(EvaluationContext context)
    {
        string typeName = context.TypeName.Split(separator: ['.'])
            .Last();

        return typeName.Contains(value: "Orchestration", comparisonType: StringComparison.Ordinal)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                new AnalysisItem
                {
                    Code = "STXO002",
                    Description = "An orchestration service name must contain the Orchestration identifier.",
                    Severity = AnalysisSeverity.Warning,
                    Type = context.TypeName,
                    LineNumber = context.LineNumber,
                },
            };
    }
}