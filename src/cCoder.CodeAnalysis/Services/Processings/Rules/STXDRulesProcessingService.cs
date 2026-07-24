// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXDRulesProcessingService : ISTXDRulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateSTXD001(context: context);
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXD001(EvaluationContext context)
    {
        bool consumesDependency = context.Dependencies.Any(
            predicate: (TypeDependency dependency) =>
                dependency.StandardElementType == StandardElementType.Dependency
                && context.LocalDependencyTypeNames.Contains(value: dependency.TypeName)
        );

        if (context.StandardElementType != StandardElementType.Broker && consumesDependency)
        {
            yield return new AnalysisItem
            {
                Code = "STXD001",
                Description = "Dependency elements may only be consumed by brokers.",
                Severity = AnalysisSeverity.Warning,
                Type = context.TypeName,
                LineNumber = context.LineNumber,
            };
        }
    }
}