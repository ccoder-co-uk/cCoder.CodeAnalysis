// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class OWASPRulesProcessingService : IOWASPRulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateOWASP0001(context: context);
    }

    private static IEnumerable<AnalysisItem> EvaluateOWASP0001(EvaluationContext context) =>
        (context.ArchitectureElement?.Methods ?? [])
            .Where(method => method.IsHttpRequestHandler
                && method.HttpResponses.Any(response => response.ExposesExceptionDetails))
            .Select(method => new AnalysisItem
            {
                Code = "OWASP0001",
                Description = "API error responses must not disclose exception messages, stack traces, or internal exception objects.",
                Severity = AnalysisSeverity.Warning,
                Type = context.TypeName,
                LineNumber = method.LineNumber > 0 ? method.LineNumber : context.LineNumber,
            });
}
