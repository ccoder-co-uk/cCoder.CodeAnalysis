// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class OWASPRulesProcessingService : IOWASPRulesProcessingService
{
    private static readonly IArchitectureModelQueriesProcessingService architectureModelQueries =
        new ArchitectureModelQueriesProcessingService();

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateOWASP0001(context: context)
            .Concat(second: EvaluateOWASP0002(context: context))
            .Concat(second: EvaluateOWASP0003(context: context));
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
                Type = architectureModelQueries.GetTypeName(context),
                LineNumber = method.LineNumber > 0
                    ? method.LineNumber
                    : architectureModelQueries.GetLineNumber(context),
            });

    private static IEnumerable<AnalysisItem> EvaluateOWASP0002(
        EvaluationContext context)
    {
        string typeName = architectureModelQueries.GetTypeName(
            context: context);

        if (typeName.EndsWith(
            value: "PasswordHashingDependency",
            comparisonType: StringComparison.Ordinal))
        {
            return [];
        }

        return (context.ArchitectureElement?.Methods ?? [])
            .Where(method =>
                method.DirectCalls.Any(call =>
                    call.TypeName.Contains(
                        value: "PasswordHasher",
                        comparisonType: StringComparison.Ordinal)
                    || call.TypeName.Contains(
                        value: "Argon2",
                        comparisonType: StringComparison.Ordinal)))
            .Select(method => new AnalysisItem
            {
                Code = "OWASP0002",
                Description = "Password derivation must be isolated in a PasswordHashingDependency behind a standard broker.",
                Severity = AnalysisSeverity.Warning,
                Type = typeName,
                LineNumber = method.LineNumber > 0
                    ? method.LineNumber
                    : architectureModelQueries.GetLineNumber(context: context)
            });
    }

    private static IEnumerable<AnalysisItem> EvaluateOWASP0003(
        EvaluationContext context) =>
        (context.ArchitectureElement?.Methods ?? [])
            .Where(method =>
                method.Name.Contains(
                    value: "Token",
                    comparisonType: StringComparison.Ordinal)
                && (method.Name.StartsWith(
                        value: "Generate",
                        comparisonType: StringComparison.Ordinal)
                    || method.Name.StartsWith(
                        value: "Create",
                        comparisonType: StringComparison.Ordinal)
                    || method.Name.StartsWith(
                        value: "Issue",
                        comparisonType: StringComparison.Ordinal))
                && architectureModelQueries.CallsTypeMatching(
                    context: context,
                    methodId: method.Id,
                    typeNameFragment: "System.Guid"))
            .Select(method => new AnalysisItem
            {
                Code = "OWASP0003",
                Description = "Security tokens must use a cryptographically secure random-number generator rather than Guid.",
                Severity = AnalysisSeverity.Warning,
                Type = architectureModelQueries.GetTypeName(context: context),
                LineNumber = method.LineNumber > 0
                    ? method.LineNumber
                    : architectureModelQueries.GetLineNumber(context: context)
            });
}