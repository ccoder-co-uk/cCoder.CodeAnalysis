// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class AggregationServiceCodeAnalysisRulesProcessingService
    : CodeAnalysisRulesProcessingService,
        IAggregationServiceCodeAnalysisRulesProcessingService
{
    public AnalysisItem[] Evaluate(EvaluationContext context)
    {
        List<AnalysisItem> list = new List<AnalysisItem>();
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateSourceFormatting(context));
        list.AddRange(AggregationServiceCodeAnalysisRulesProcessingService.EvaluateSTXA001(context));
        list.AddRange(AggregationServiceCodeAnalysisRulesProcessingService.EvaluateSTXA002(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluatePropertiesAreNotAllowed(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateRedundantPassThroughService(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateFlowForward(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluatePublicApiFlowForward(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateBusinessImplementationVisibility(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateSingleServiceContract(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateServiceContractPattern(context));
        return list.ToArray();
    }

    private static AnalysisItem[] EvaluateSTXA001(EvaluationContext context)
    {
        bool hasSingleDependencyVariation = context
            .Dependencies.Select(
                (TypeDependency dependency) => dependency.StandardElementType
            )
            .Distinct()
            .Count() <= 1;

        return hasSingleDependencyVariation
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXA001",
                    "An aggregation service may have any number of dependencies, but they must share the same service variation.",
                    context
                ),
            };
    }

    private static AnalysisItem[] EvaluateSTXA002(EvaluationContext context)
    {
        string typeName = context.TypeName.Split('.').Last();
        return typeName.Contains("Aggregation", StringComparison.Ordinal)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXA002",
                    "An aggregation service name must contain the Aggregation identifier.",
                    context
                ),
            };
    }
}
