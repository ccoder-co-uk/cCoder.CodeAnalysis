// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXMGRulesProcessingService : ISTXMGRulesProcessingService
{
    private readonly IArchitectureModelQueriesProcessingService architectureModelQueries =
        new ArchitectureModelQueriesProcessingService();

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateSTXMG001(context: context)
            .Concat(second: EvaluateSTXMG002(context: context));
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

    private IEnumerable<AnalysisItem> EvaluateSTXMG001(EvaluationContext context)
    {
        IReadOnlyList<TypeDependency> dependencies =
            architectureModelQueries.GetDependencies(context: context);
        int dependencyCount = dependencies.Count;
        bool hasValidCount = dependencyCount is 2 or 3;

        bool containsOnlyCoordinations = dependencies.All(
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
