// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXORulesProcessingService : ISTXORulesProcessingService
{
    private static readonly IArchitectureModelQueriesProcessingService architectureModelQueries =
        new ArchitectureModelQueriesProcessingService();

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateSTXO001(context: context)
            .Concat(second: EvaluateSTXO002(context: context));
    }

    private IEnumerable<AnalysisItem> EvaluateSTXO001(EvaluationContext context)
    {
        IReadOnlyList<TypeDependency> dependencies =
            architectureModelQueries.GetDependencies(context: context);
        int count = dependencies.Count;
        bool flag = (uint)(count - 2) <= 1u;
        bool hasValidCount = flag;

        bool containsFoundation = dependencies.Any(
            predicate: (TypeDependency dependency) =>
                dependency.StandardElementType == StandardElementType.FoundationService
        );

        bool containsProcessing = dependencies.Any(
            predicate: (TypeDependency dependency) =>
                dependency.StandardElementType == StandardElementType.ProcessingService
        );

        bool containsOnlySupportedDependencies = dependencies.All(
            predicate: delegate (TypeDependency dependency)
            {
                StandardElementType standardElementType = dependency.StandardElementType;
                return standardElementType
                    is StandardElementType.FoundationService
                    or StandardElementType.ProcessingService;
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
                    Type = architectureModelQueries.GetTypeName(context: context),
                    LineNumber = architectureModelQueries.GetLineNumber(context: context),
                },
            };
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXO002(EvaluationContext context)
    {
        string typeName = architectureModelQueries.GetTypeName(context: context).Split(separator: ['.'])
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
                    Type = architectureModelQueries.GetTypeName(context: context),
                    LineNumber = architectureModelQueries.GetLineNumber(context: context),
                },
            };
    }
}
