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
    }

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
}
