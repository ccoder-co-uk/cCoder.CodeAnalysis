// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXARulesProcessingService : ISTXARulesProcessingService
{
    private readonly IArchitectureModelQueriesProcessingService architectureModelQueries =
        new ArchitectureModelQueriesProcessingService();

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateSTXA001(context: context)
            .Concat(second: EvaluateSTXA002(context: context));
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

    private IEnumerable<AnalysisItem> EvaluateSTXA001(EvaluationContext context)
    {
        bool hasSingleDependencyVariation =
            architectureModelQueries
                .GetDependencies(context: context)
                .Where(predicate: (TypeDependency dependency) =>
                    IsServiceVariation(
                        standardElementType: dependency.StandardElementType))
                .Select(selector: (TypeDependency dependency) =>
                    dependency.StandardElementType)
                .Distinct()
                .Count() <= 1;

        return hasSingleDependencyVariation
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: "STXA001",
                    description: "An aggregation service may have any number of dependencies, but they must share the same service variation.",
                    context: context
                ),
            };
    }

    private static bool IsServiceVariation(
        StandardElementType standardElementType) =>
        standardElementType is
            StandardElementType.FoundationService or
            StandardElementType.ProcessingService or
            StandardElementType.OrchestrationService or
            StandardElementType.CoordinationService or
            StandardElementType.ManagementService or
            StandardElementType.AggregationService;

    private static IEnumerable<AnalysisItem> EvaluateSTXA002(EvaluationContext context)
    {
        string typeName = context.TypeName.Split(separator: ['.'])
            .Last();

        return typeName.Contains(value: "Aggregation", comparisonType: StringComparison.Ordinal)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: "STXA002",
                    description: "An aggregation service name must contain the Aggregation identifier.",
                    context: context
                ),
            };
    }
}
