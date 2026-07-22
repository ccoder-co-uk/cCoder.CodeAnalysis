// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class DependencyCodeAnalysisRulesProcessingService
    : CodeAnalysisRulesProcessingService,
        IDependencyCodeAnalysisRulesProcessingService
{
    public AnalysisItem[] Evaluate(EvaluationContext context)
    {
        return CodeAnalysisRulesProcessingService.EvaluateSourceFormatting(context);
    }
}