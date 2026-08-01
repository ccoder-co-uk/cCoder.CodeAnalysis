// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class RFCRulesProcessingService : IRFCRulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateRFC0001(context: context)
            .Concat(second: EvaluateRFC0002(context: context))
            .Concat(second: EvaluateRFC0003(context: context))
            .Concat(second: EvaluateRFC0004(context: context))
            .Concat(second: EvaluateRFC0005(context: context))
            .Concat(second: EvaluateRFC0006(context: context))
            .Concat(second: EvaluateRFC0007(context: context))
            .Concat(second: EvaluateRFC0008(context: context))
            .Concat(second: EvaluateRFC0009(context: context))
            .Concat(second: EvaluateRFC0010(context: context));
    }

    private static IEnumerable<AnalysisItem> EvaluateRFC0001(EvaluationContext context) =>
        GetODataMethods(context)
            .Where(method => method.HttpMethods.Contains("POST", StringComparer.Ordinal)
                && method.Name == "Post"
                && method.HasFromBodyParameter
                && !HasSuccessResponse(method, 201))
            .Select(method => CreateAnalysisItem(
                "RFC0001",
                "An OData CRUD Post action must return 201 Created with the created representation.",
                context,
                method));

    private static IEnumerable<AnalysisItem> EvaluateRFC0002(EvaluationContext context) =>
        GetODataMethods(context)
            .Where(method => method.HttpMethods.Contains("DELETE", StringComparer.Ordinal)
                && method.Name == "Delete"
                && !HasSuccessResponse(method, 204))
            .Select(method => CreateAnalysisItem(
                "RFC0002",
                "An OData CRUD Delete action must return 204 No Content when deletion succeeds.",
                context,
                method));

    private static IEnumerable<AnalysisItem> EvaluateRFC0003(EvaluationContext context) =>
        GetODataMethods(context)
            .Where(method => method.HttpMethods.Contains("GET", StringComparer.Ordinal)
                && method.Name is "Get" or "GetAll"
                && !HasSuccessResponse(method, 200))
            .Select(method => CreateAnalysisItem(
                "RFC0003",
                "An OData CRUD Get action must return 200 OK with the requested representation.",
                context,
                method));

    private static IEnumerable<AnalysisItem> EvaluateRFC0004(EvaluationContext context) =>
        GetODataMethods(context)
            .Where(method => (method.HttpMethods.Contains("PUT", StringComparer.Ordinal)
                    || method.HttpMethods.Contains("PATCH", StringComparer.Ordinal))
                && method.Name is "Put" or "Patch"
                && !HasSuccessResponse(method, 200))
            .Select(method => CreateAnalysisItem(
                "RFC0004",
                "An OData CRUD Put or Patch action that returns the updated representation must return 200 OK.",
                context,
                method));

    private static IEnumerable<AnalysisItem> EvaluateRFC0005(EvaluationContext context) =>
        GetHttpMethods(context)
            .Where(method => HasEscapingException(method, "Validation")
                && !HasExceptionResponse(method, "Validation", 400, 422))
            .Select(method => CreateAnalysisItem(
                "RFC0005",
                "An HTTP validation failure must return 400 Bad Request or the adopted 422 semantic-validation response.",
                context,
                method));

    private static IEnumerable<AnalysisItem> EvaluateRFC0006(EvaluationContext context) =>
        GetHttpMethods(context)
            .Where(method => HasEscapingException(method, "Authentication")
                && !method.HttpResponses.Any(response => response.IsExceptionPath
                    && response.ExceptionType.Contains("Authentication", StringComparison.Ordinal)
                    && response.StatusCode == 401
                    && response.ResultMethod == "Challenge"))
            .Select(method => CreateAnalysisItem(
                "RFC0006",
                "An authentication failure must return 401 Unauthorized with an authentication challenge.",
                context,
                method));

    private static IEnumerable<AnalysisItem> EvaluateRFC0007(EvaluationContext context) =>
        GetHttpMethods(context)
            .Where(method => HasEscapingException(method, "Authorization")
                && !HasExceptionResponse(method, "Authorization", 403))
            .Select(method => CreateAnalysisItem(
                "RFC0007",
                "An authenticated caller denied an operation must receive 403 Forbidden.",
                context,
                method));

    private static IEnumerable<AnalysisItem> EvaluateRFC0008(EvaluationContext context) =>
        GetHttpMethods(context)
            .Where(method => method.IsODataControllerAction
                && method.HttpMethods.Contains("GET", StringComparer.Ordinal)
                && method.HasKeyParameter
                && !method.HandlesNullWithNotFound)
            .Select(method => CreateAnalysisItem(
                "RFC0008",
                "A keyed OData retrieval must return 404 Not Found when no entity exists.",
                context,
                method));

    private static IEnumerable<AnalysisItem> EvaluateRFC0009(EvaluationContext context) =>
        GetHttpMethods(context)
            .Where(method => HasConflictException(method)
                && !HasExceptionResponse(method, "Conflict", 409)
                && !HasExceptionResponse(method, "Concurrency", 409))
            .Select(method => CreateAnalysisItem(
                "RFC0009",
                "A non-precondition state or concurrency conflict must return 409 Conflict.",
                context,
                method));

    private static IEnumerable<AnalysisItem> EvaluateRFC0010(EvaluationContext context) =>
        GetHttpMethods(context)
            .Where(method => method.HttpResponses.Any(response => response.IsExceptionPath
                && response.ExceptionType == "System.Exception"
                && response.StatusCode != 500))
            .Select(method => CreateAnalysisItem(
                "RFC0010",
                "An unclassified HTTP failure must be rethrown to approved terminal handling or return 500, never a successful or client-error response.",
                context,
                method));

    private static IEnumerable<Method> GetODataMethods(EvaluationContext context) =>
        (context.ArchitectureElement?.Methods ?? [])
            .Where(method => method.IsODataControllerAction);

    private static IEnumerable<Method> GetHttpMethods(EvaluationContext context) =>
        (context.ArchitectureElement?.Methods ?? [])
            .Where(method => method.IsHttpRequestHandler);

    private static bool HasEscapingException(Method method, string category) =>
        (method.PossibleExceptionTypes ?? method.ThrowsExceptionTypes).Any(
            exceptionType => exceptionType.Contains(category, StringComparison.Ordinal));

    private static bool HasConflictException(Method method) =>
        (method.PossibleExceptionTypes ?? method.ThrowsExceptionTypes).Any(exceptionType =>
            (exceptionType.Contains("Conflict", StringComparison.Ordinal)
                || exceptionType.Contains("Concurrency", StringComparison.Ordinal))
            && !exceptionType.Contains("Precondition", StringComparison.Ordinal)
            && !exceptionType.Contains("ETag", StringComparison.OrdinalIgnoreCase));

    private static bool HasExceptionResponse(Method method, string category, params int[] statusCodes) =>
        method.HttpResponses.Any(response => response.IsExceptionPath
            && response.ExceptionType.Contains(category, StringComparison.Ordinal)
            && statusCodes.Contains(response.StatusCode));

    private static bool HasSuccessResponse(Method method, int statusCode) =>
        method.HttpResponses.Any(response => !response.IsExceptionPath
            && response.StatusCode == statusCode);

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
