// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class ModelCodeAnalysisRulesProcessingService
    : CodeAnalysisRulesProcessingService,
        IModelCodeAnalysisRulesProcessingService
{
    public AnalysisItem[] Evaluate(EvaluationContext context)
    {
        AnalysisItem[] array = CodeAnalysisRulesProcessingService.EvaluateSourceFormatting(context);
        AnalysisItem[] array2 = ModelCodeAnalysisRulesProcessingService.EvaluateSTXM001(context);
        int num = 0;
        AnalysisItem[] array3 = new AnalysisItem[array.Length + array2.Length];
        ReadOnlySpan<AnalysisItem> readOnlySpan = new ReadOnlySpan<AnalysisItem>(array);
        readOnlySpan.CopyTo(new Span<AnalysisItem>(array3).Slice(num, readOnlySpan.Length));
        num += readOnlySpan.Length;
        ReadOnlySpan<AnalysisItem> readOnlySpan2 = new ReadOnlySpan<AnalysisItem>(array2);
        readOnlySpan2.CopyTo(new Span<AnalysisItem>(array3).Slice(num, readOnlySpan2.Length));
        return array3;
    }

    private static AnalysisItem[] EvaluateSTXM001(EvaluationContext context)
    {
        return (
            from method in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                .OfType<MethodDeclarationSyntax>()
            select CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                "STXM001",
                "Models must not declare methods.",
                context,
                method.GetLocation()
            )
        ).ToArray();
    }
}