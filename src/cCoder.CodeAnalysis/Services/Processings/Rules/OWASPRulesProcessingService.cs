// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class OWASPRulesProcessingService : IOWASPRulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        if (context.ArchitectureElement is null)
        {
            yield break;
        }

        foreach (Method method in context.ArchitectureElement.Methods
            .Where(method => method.IsHttpRequestHandler))
        {
            if (method.HttpResponses.Any(response => response.ExposesExceptionDetails))
            {
                yield return new AnalysisItem
                {
                    Code = "OWASP0004",
                    Description = "API error responses must not disclose exception messages, stack traces, or internal exception objects.",
                    Severity = AnalysisSeverity.Warning,
                    Type = context.TypeName,
                    LineNumber = method.LineNumber > 0 ? method.LineNumber : context.LineNumber,
                };
            }
        }
    }
}
