// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXCRulesProcessingService : ISTXCRulesProcessingService
{
    private static readonly IArchitectureModelQueriesProcessingService architectureModelQueries =
        new ArchitectureModelQueriesProcessingService();

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateSTXC001(context: context)
            .Concat(second: EvaluateSTXC002(context: context));
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
            Type = architectureModelQueries.GetTypeName(context: context),
            LineNumber = location is null ? architectureModelQueries.GetLineNumber(context: context) : location.GetLineSpan().StartLinePosition.Line + 1,
        };
    }

    private IEnumerable<AnalysisItem> EvaluateSTXC001(EvaluationContext context)
    {
        IReadOnlyList<TypeDependency> dependencies =
            architectureModelQueries.GetDependencies(context: context);

        int dependencyCount = dependencies.Count;
        bool hasValidCount = dependencyCount is 2 or 3;

        bool containsOnlyOrchestrations = dependencies.All(
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
        string typeName = architectureModelQueries.GetTypeName(context: context).Split(separator: ['.'])
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
