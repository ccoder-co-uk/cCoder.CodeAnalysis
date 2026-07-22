// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class CoordinationServiceCodeAnalysisRulesProcessingService
    : CodeAnalysisRulesProcessingService,
        ICoordinationServiceCodeAnalysisRulesProcessingService
{
    public AnalysisItem[] Evaluate(EvaluationContext context)
    {
        List<AnalysisItem> list = new List<AnalysisItem>();
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateSourceFormatting(context));
        list.AddRange(CoordinationServiceCodeAnalysisRulesProcessingService.EvaluateSTXC001(context));
        list.AddRange(CoordinationServiceCodeAnalysisRulesProcessingService.EvaluateSTXC002(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluatePropertiesAreNotAllowed(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateRedundantPassThroughService(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateFlowForward(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluatePublicApiFlowForward(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateBusinessImplementationVisibility(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateSingleServiceContract(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateServiceContractPattern(context));
        return list.ToArray();
    }

    private static AnalysisItem[] EvaluateSTXC001(EvaluationContext context)
    {
        return CodeAnalysisRulesProcessingService.EvaluateDependencyLayer(
            context,
            StandardElementType.OrchestrationService,
            "STXC001"
        );
    }

    private static AnalysisItem[] EvaluateSTXC002(EvaluationContext context)
    {
        string typeName = context.TypeName.Split('.').Last();
        return typeName.Contains("Coordination", StringComparison.Ordinal)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXC002",
                    "A coordination service name must contain the Coordination identifier.",
                    context
                ),
            };
    }
}