// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class ManagementServiceCodeAnalysisRulesProcessingService
    : CodeAnalysisRulesProcessingService,
        IManagementServiceCodeAnalysisRulesProcessingService
{
    public AnalysisItem[] Evaluate(EvaluationContext context)
    {
        List<AnalysisItem> list = new List<AnalysisItem>();
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateSourceFormatting(context));
        list.AddRange(ManagementServiceCodeAnalysisRulesProcessingService.EvaluateSTXMG001(context));
        list.AddRange(ManagementServiceCodeAnalysisRulesProcessingService.EvaluateSTXMG002(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluatePropertiesAreNotAllowed(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateRedundantPassThroughService(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateFlowForward(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluatePublicApiFlowForward(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateBusinessImplementationVisibility(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateSingleServiceContract(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateServiceContractPattern(context));
        return list.ToArray();
    }

    private static AnalysisItem[] EvaluateSTXMG001(EvaluationContext context)
    {
        return CodeAnalysisRulesProcessingService.EvaluateDependencyLayer(
            context,
            StandardElementType.CoordinationService,
            "STXMG001"
        );
    }

    private static AnalysisItem[] EvaluateSTXMG002(EvaluationContext context)
    {
        string typeName = context.TypeName.Split('.').Last();
        return typeName.Contains("Management", StringComparison.Ordinal)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXMG002",
                    "A management service name must contain the Management identifier.",
                    context
                ),
            };
    }
}