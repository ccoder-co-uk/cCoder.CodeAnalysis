// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class RFCRulesProcessingService : IRFCRulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        foreach (TypeDeclarationSyntax declaration in context.Declarations
            .Where(predicate: IsODataController))
        {
            foreach (MethodDeclarationSyntax method in declaration.Members
                .OfType<MethodDeclarationSyntax>())
            {
                AnalysisItem? item = EvaluateMethod(
                    context: context,
                    method: method);

                if (item is not null)
                {
                    yield return item;
                }
            }
        }

        if (context.ArchitectureElement is not null)
        {
            foreach (AnalysisItem item in EvaluateModelRules(context: context))
            {
                yield return item;
            }
        }
    }

    private static IEnumerable<AnalysisItem> EvaluateModelRules(
        EvaluationContext context)
    {
        foreach (Method method in context.ArchitectureElement.Methods
            .Where(method => method.IsHttpRequestHandler))
        {
            if (HasEscapingException(method: method, category: "Validation")
                && !HasExceptionResponse(method: method, category: "Validation", 400, 422))
            {
                yield return CreateModelAnalysisItem(
                    code: "RFC0005",
                    description: "An HTTP validation failure must return 400 Bad Request or the adopted 422 semantic-validation response.",
                    context: context,
                    method: method);
            }

            if (HasEscapingException(method: method, category: "Authentication")
                && !method.HttpResponses.Any(
                    response => response.IsExceptionPath
                        && response.ExceptionType.Contains("Authentication", StringComparison.Ordinal)
                        && response.StatusCode == 401
                        && response.ResultMethod == "Challenge"))
            {
                yield return CreateModelAnalysisItem(
                    code: "RFC0006",
                    description: "An authentication failure must return 401 Unauthorized with an authentication challenge.",
                    context: context,
                    method: method);
            }

            if (HasEscapingException(method: method, category: "Authorization")
                && !HasExceptionResponse(method: method, category: "Authorization", 403))
            {
                yield return CreateModelAnalysisItem(
                    code: "RFC0007",
                    description: "An authenticated caller denied an operation must receive 403 Forbidden.",
                    context: context,
                    method: method);
            }

            if (method.IsODataControllerAction
                && method.HttpMethods.Contains("GET", StringComparer.Ordinal)
                && method.HasKeyParameter
                && !method.HandlesNullWithNotFound)
            {
                yield return CreateModelAnalysisItem(
                    code: "RFC0008",
                    description: "A keyed OData retrieval must return 404 Not Found when no entity exists.",
                    context: context,
                    method: method);
            }

            if (HasConflictException(method: method)
                && !HasExceptionResponse(method: method, category: "Conflict", 409)
                && !HasExceptionResponse(method: method, category: "Concurrency", 409))
            {
                yield return CreateModelAnalysisItem(
                    code: "RFC0009",
                    description: "A non-precondition state or concurrency conflict must return 409 Conflict.",
                    context: context,
                    method: method);
            }

            if (method.HttpResponses.Any(
                response => response.IsExceptionPath
                    && response.ExceptionType == "System.Exception"
                    && response.StatusCode != 500))
            {
                yield return CreateModelAnalysisItem(
                    code: "RFC0010",
                    description: "An unclassified HTTP failure must be rethrown to approved terminal handling or return 500, never a successful or client-error response.",
                    context: context,
                    method: method);
            }
        }
    }

    private static bool HasEscapingException(
        Method method,
        string category) =>

        (method.PossibleExceptionTypes ?? method.ThrowsExceptionTypes).Any(
            exceptionType => exceptionType.Contains(category, StringComparison.Ordinal));

    private static bool HasConflictException(Method method) =>

        (method.PossibleExceptionTypes ?? method.ThrowsExceptionTypes).Any(
            exceptionType =>
                (exceptionType.Contains("Conflict", StringComparison.Ordinal)
                    || exceptionType.Contains("Concurrency", StringComparison.Ordinal))
                && !exceptionType.Contains("Precondition", StringComparison.Ordinal)
                && !exceptionType.Contains("ETag", StringComparison.OrdinalIgnoreCase));

    private static bool HasExceptionResponse(
        Method method,
        string category,
        params int[] statusCodes) =>

        method.HttpResponses.Any(
            response => response.IsExceptionPath
                && response.ExceptionType.Contains(category, StringComparison.Ordinal)
                && statusCodes.Contains(response.StatusCode));

    private static AnalysisItem? EvaluateMethod(
        EvaluationContext context,
        MethodDeclarationSyntax method)
    {
        string methodName = method.Identifier.Text;

        if (methodName == "Post" && HasFromBodyParameter(method: method))
        {
            return HasReturnedResult(method: method, "Created", "CreatedAtAction", "CreatedAtRoute")
                ? null
                : CreateAnalysisItem(
                    code: "RFC0001",
                    description: "An OData CRUD Post action must return 201 Created with the created representation.",
                    context: context,
                    method: method);
        }

        if (methodName == "Delete")
        {
            return HasReturnedResult(method: method, "NoContent")
                ? null
                : CreateAnalysisItem(
                    code: "RFC0002",
                    description: "An OData CRUD Delete action must return 204 No Content when deletion succeeds.",
                    context: context,
                    method: method);
        }

        if (methodName is "Get" or "GetAll")
        {
            return HasReturnedResult(method: method, "Ok")
                ? null
                : CreateAnalysisItem(
                    code: "RFC0003",
                    description: "An OData CRUD Get action must return 200 OK with the requested representation.",
                    context: context,
                    method: method);
        }

        if (methodName is "Put" or "Patch")
        {
            return HasReturnedResult(method: method, "Ok", "Updated")
                ? null
                : CreateAnalysisItem(
                    code: "RFC0004",
                    description: "An OData CRUD Put or Patch action that returns the updated representation must return 200 OK.",
                    context: context,
                    method: method);
        }

        return null;
    }

    private static bool IsODataController(
        TypeDeclarationSyntax declaration) =>
        declaration.BaseList?.Types.Any(
            predicate: baseType =>
                baseType.Type.ToString()
                    .Split(separator: '.')
                    .Last()
                    .Equals(
                        value: "ODataController",
                        comparisonType: StringComparison.Ordinal)) == true;

    private static bool HasFromBodyParameter(
        MethodDeclarationSyntax method) =>
        method.ParameterList.Parameters.Any(
            predicate: parameter => parameter.AttributeLists
                .SelectMany(selector: attributes => attributes.Attributes)
                .Any(predicate: attribute =>
                    attribute.Name.ToString()
                        .Split(separator: '.')
                        .Last()
                        .Equals(
                            value: "FromBody",
                            comparisonType: StringComparison.Ordinal)));

    private static bool HasReturnedResult(
        MethodDeclarationSyntax method,
        params string[] resultNames) =>
        GetReturnedExpressions(method: method)
            .SelectMany(selector: expression => expression
                .DescendantNodesAndSelf()
                .OfType<InvocationExpressionSyntax>())
            .Select(selector: GetInvocationName)
            .Any(predicate: invocationName => resultNames.Contains(
                value: invocationName,
                comparer: StringComparer.Ordinal));

    private static IEnumerable<ExpressionSyntax> GetReturnedExpressions(
        MethodDeclarationSyntax method)
    {
        if (method.ExpressionBody?.Expression is ExpressionSyntax expression)
        {
            yield return expression;
        }

        foreach (ReturnStatementSyntax returnStatement in
            method.DescendantNodes().OfType<ReturnStatementSyntax>())
        {
            if (returnStatement.Expression is ExpressionSyntax returnedExpression)
            {
                yield return returnedExpression;
            }
        }
    }

    private static string GetInvocationName(
        InvocationExpressionSyntax invocation) =>
        invocation.Expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            MemberAccessExpressionSyntax memberAccess =>
                memberAccess.Name.Identifier.Text,
            _ => string.Empty,
        };

    private static AnalysisItem CreateAnalysisItem(
        string code,
        string description,
        EvaluationContext context,
        MethodDeclarationSyntax method) =>
        new()
        {
            Code = code,
            Description = description,
            Severity = AnalysisSeverity.Warning,
            Type = context.TypeName,
            LineNumber = method.GetLocation()
                .GetLineSpan()
                .StartLinePosition.Line + 1,
        };

    private static AnalysisItem CreateModelAnalysisItem(
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
