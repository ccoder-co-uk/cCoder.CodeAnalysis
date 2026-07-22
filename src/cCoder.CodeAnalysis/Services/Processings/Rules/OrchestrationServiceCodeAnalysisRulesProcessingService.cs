// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class OrchestrationServiceCodeAnalysisRulesProcessingService
    : CodeAnalysisRulesProcessingService,
        IOrchestrationServiceCodeAnalysisRulesProcessingService
{
    public AnalysisItem[] Evaluate(EvaluationContext context)
    {
        List<AnalysisItem> list = new List<AnalysisItem>();
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateSourceFormatting(context));
        list.AddRange(OrchestrationServiceCodeAnalysisRulesProcessingService.EvaluateSTXO001(context));
        list.AddRange(OrchestrationServiceCodeAnalysisRulesProcessingService.EvaluateSTXO002(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluatePropertiesAreNotAllowed(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateRedundantPassThroughService(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateFlowForward(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluatePublicApiFlowForward(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateBusinessImplementationVisibility(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateSingleServiceContract(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateServiceContractPattern(context));
        return list.ToArray();
    }

    private static AnalysisItem[] EvaluateSTXO001(EvaluationContext context)
    {
        int count = context.Dependencies.Count;
        bool flag = (uint)(count - 2) <= 1u;
        bool hasValidCount = flag;
        bool containsFoundation = context.Dependencies.Any(
            (TypeDependency dependency) => dependency.StandardElementType == StandardElementType.FoundationService
        );
        bool containsProcessing = context.Dependencies.Any(
            (TypeDependency dependency) => dependency.StandardElementType == StandardElementType.ProcessingService
        );
        bool containsOnlySupportedDependencies = context.Dependencies.All(
            delegate(TypeDependency dependency)
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

    private static AnalysisItem[] EvaluateSTXO002(EvaluationContext context)
    {
        string typeName = context.TypeName.Split('.').Last();
        return typeName.Contains("Orchestration", StringComparison.Ordinal)
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