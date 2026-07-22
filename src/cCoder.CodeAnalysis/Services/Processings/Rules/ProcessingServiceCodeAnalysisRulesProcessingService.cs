// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class ProcessingServiceCodeAnalysisRulesProcessingService
    : CodeAnalysisRulesProcessingService,
        IProcessingServiceCodeAnalysisRulesProcessingService
{
    public AnalysisItem[] Evaluate(EvaluationContext context)
    {
        List<AnalysisItem> list = new List<AnalysisItem>();
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateSourceFormatting(context));
        list.AddRange(ProcessingServiceCodeAnalysisRulesProcessingService.EvaluateSTXP001(context));
        list.AddRange(ProcessingServiceCodeAnalysisRulesProcessingService.EvaluateSTXP002(context));
        list.AddRange(ProcessingServiceCodeAnalysisRulesProcessingService.EvaluateSTXP003(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluatePropertiesAreNotAllowed(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateRedundantPassThroughService(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateFlowForward(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluatePublicApiFlowForward(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateBusinessImplementationVisibility(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateSingleServiceContract(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateServiceContractPattern(context));
        return list.ToArray();
    }

    private static AnalysisItem[] EvaluateSTXP001(EvaluationContext context)
    {
        int foundationCount = context.Dependencies.Count(
            (TypeDependency dependency) => dependency.StandardElementType == StandardElementType.FoundationService
        );
        bool hasUnsupportedServiceDependency = context.Dependencies.Any(
            delegate(TypeDependency dependency)
            {
                StandardElementType standardElementType = dependency.StandardElementType;
                return (uint)(standardElementType - 3) <= 4u;
            }
        );
        return (!(foundationCount > 1 || hasUnsupportedServiceDependency))
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXP001",
                    "A processing service may use only one foundation service and no higher-level service.",
                    context
                ),
            };
    }

    private static AnalysisItem[] EvaluateSTXP002(EvaluationContext context)
    {
        string typeName = context.TypeName.Split('.').Last();
        return typeName.Contains("Processing", StringComparison.Ordinal)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXP002",
                    "A processing service name must contain the Processing identifier.",
                    context
                ),
            };
    }

    private static AnalysisItem[] EvaluateSTXP003(EvaluationContext context)
    {
        TypeDependency[] foundationDependencies = context
            .Dependencies.Where(
                (TypeDependency dependency) => dependency.StandardElementType == StandardElementType.FoundationService
            )
            .ToArray();
        if (foundationDependencies.Length != 1)
        {
            return Array.Empty<AnalysisItem>();
        }
        string serviceName = context.TypeName.Split('.').Last();
        string foundationName = foundationDependencies.Single().TypeName.Split('.').Last();
        string entityName = foundationName.TrimStart('I').Replace("Service", string.Empty);
        return serviceName.Contains(entityName, StringComparison.Ordinal)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXP003",
                    "A processing service name must identify the entity of its foundation dependency.",
                    context
                ),
            };
    }
}