// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXPRulesProcessingService : ISTXPRulesProcessingService
{
    private readonly IArchitectureModelQueriesProcessingService architectureModelQueries =
        new ArchitectureModelQueriesProcessingService();

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        foreach (AnalysisItem item in EvaluateSTXP001(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXP002(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXP003(context: context))
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

    private IEnumerable<AnalysisItem> EvaluateSTXP001(EvaluationContext context)
    {
        IReadOnlyList<TypeDependency> dependencies =
            architectureModelQueries.GetDependencies(context: context);
        int foundationCount = dependencies.Count(
            predicate: (TypeDependency dependency) =>
                dependency.StandardElementType == StandardElementType.FoundationService
        );

        bool hasUnsupportedServiceDependency = dependencies.Any(
            predicate: delegate (TypeDependency dependency)
            {
                StandardElementType standardElementType = dependency.StandardElementType;
                return (uint)(standardElementType - 3) <= 4u;
            }
        );

        return (!(foundationCount > 1 || hasUnsupportedServiceDependency))
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: "STXP001",
                    description: "A processing service may use only one foundation service and no higher-level service.",
                    context: context
                ),
            };
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXP002(EvaluationContext context)
    {
        string typeName = context.TypeName.Split(separator: ['.'])
            .Last();

        return typeName.Contains(value: "Processing", comparisonType: StringComparison.Ordinal)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: "STXP002",
                    description: "A processing service name must contain the Processing identifier.",
                    context: context
                ),
            };
    }

    private IEnumerable<AnalysisItem> EvaluateSTXP003(EvaluationContext context)
    {
        TypeDependency[] foundationDependencies = architectureModelQueries
            .GetDependencies(context: context)
            .Where(
                predicate: (TypeDependency dependency) =>
                    dependency.StandardElementType == StandardElementType.FoundationService
            )
            .ToArray();

        if (foundationDependencies.Length != 1)
        {
            return Array.Empty<AnalysisItem>();
        }

        string serviceName = RemoveGenericTypeArguments(typeName: context.TypeName.Split(separator: ['.'])
            .Last());

        string foundationName = RemoveGenericTypeArguments(
            typeName: foundationDependencies.Single().TypeName.Split(separator: '.')
            .Last()
        );

        string entityName = foundationName
            .TrimStart(trimChars: ['I'])
            .Replace(oldValue: "Service", newValue: string.Empty);

        return serviceName.Contains(value: entityName, comparisonType: StringComparison.Ordinal)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: "STXP003",
                    description: "A processing service name must identify the entity of its foundation dependency.",
                    context: context
                ),
            };
    }

    internal static string RemoveGenericTypeArguments(string typeName)
    {
        int genericArgumentsStart = typeName.IndexOf(value: "<", comparisonType: StringComparison.Ordinal);
        return genericArgumentsStart < 0 ? typeName : typeName.Substring(startIndex: 0, length: genericArgumentsStart);
    }
}
