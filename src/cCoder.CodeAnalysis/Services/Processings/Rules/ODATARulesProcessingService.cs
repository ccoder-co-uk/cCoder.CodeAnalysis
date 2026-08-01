// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class ODATARulesProcessingService : IODATARulesProcessingService
{
    private static readonly IArchitectureModelQueriesProcessingService architectureModelQueries =
        new ArchitectureModelQueriesProcessingService();

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateODATA0001(context: context)
            .Concat(second: EvaluateODATA0002(context: context))
            .Concat(second: EvaluateODATA0003(context: context));
    }

    private static IEnumerable<AnalysisItem> EvaluateODATA0001(EvaluationContext context) =>
        GetODataMethods(context)
            .Where(method => method.HttpMethods.Contains("POST", StringComparer.Ordinal)
                && !method.HttpResponses.Any(response => response.StatusCode == 201))
            .Select(method => CreateAnalysisItem(
                "ODATA0001",
                "An OData entity creation must return the created resource in a 201 response.",
                context,
                method));

    private static IEnumerable<AnalysisItem> EvaluateODATA0002(EvaluationContext context) =>
        GetODataMethods(context)
            .Where(method => method.HttpMethods.Contains("GET", StringComparer.Ordinal)
                && method.HasKeyParameter
                && !method.HandlesNullWithNotFound)
            .Select(method => CreateAnalysisItem(
                "ODATA0002",
                "A request for a non-existent OData entity URL must return 404 Not Found.",
                context,
                method));

    private static IEnumerable<AnalysisItem> EvaluateODATA0003(EvaluationContext context) =>
        GetODataMethods(context)
            .Where(method => (method.PossibleExceptionTypes ?? method.ThrowsExceptionTypes).Any(
                    exceptionType => exceptionType.EndsWith("NotImplementedException", StringComparison.Ordinal))
                && !method.HttpResponses.Any(response => response.StatusCode == 501))
            .Select(method => CreateAnalysisItem(
                "ODATA0003",
                "Recognized but unsupported OData functionality must return 501 Not Implemented.",
                context,
                method));

    private static IEnumerable<Method> GetODataMethods(EvaluationContext context) =>
        (context.ArchitectureElement?.Methods ?? [])
            .Where(method => method.IsODataControllerAction);

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
            Type = architectureModelQueries.GetTypeName(context),
            LineNumber = method.LineNumber > 0
                ? method.LineNumber
                : architectureModelQueries.GetLineNumber(context),
        };
}
