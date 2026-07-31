// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class ODATARulesProcessingService : IODATARulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        if (context.ArchitectureElement is null)
        {
            yield break;
        }

        foreach (Method method in context.ArchitectureElement.Methods
            .Where(method => method.IsODataControllerAction))
        {
            if (method.HttpMethods.Contains("POST", StringComparer.Ordinal)
                && !method.HttpResponses.Any(response => response.StatusCode == 201))
            {
                yield return CreateAnalysisItem(
                    code: "ODATA0001",
                    description: "An OData entity creation must return the created resource in a 201 response.",
                    context: context,
                    method: method);
            }

            if (method.HttpMethods.Contains("GET", StringComparer.Ordinal)
                && method.HasKeyParameter
                && !method.HandlesNullWithNotFound)
            {
                yield return CreateAnalysisItem(
                    code: "ODATA0002",
                    description: "A request for a non-existent OData entity URL must return 404 Not Found.",
                    context: context,
                    method: method);
            }

            if (method.ThrowsExceptionTypes.Any(
                    exceptionType => exceptionType.EndsWith(
                        "NotImplementedException",
                        StringComparison.Ordinal))
                && !method.HttpResponses.Any(response => response.StatusCode == 501))
            {
                yield return CreateAnalysisItem(
                    code: "ODATA0003",
                    description: "Recognized but unsupported OData functionality must return 501 Not Implemented.",
                    context: context,
                    method: method);
            }
        }
    }

    private static AnalysisItem CreateAnalysisItem(
        string code,
        string description,
        EvaluationContext context,
        Method method) =>

        new()
        {
            Code = code,
            Description = description,
            Severity = AnalysisSeverity.Warning,
            Type = context.TypeName,
            LineNumber = method.LineNumber > 0 ? method.LineNumber : context.LineNumber,
        };
}
