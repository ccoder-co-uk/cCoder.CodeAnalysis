// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXERulesProcessingService : ISTXERulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        string typeName = context.TypeName
            .Split(separator: ['.'])
            .Last();

        if (typeName.Contains(
            "Configuration",
            StringComparison.Ordinal)
            && typeName.EndsWith(
                "Extensions",
                StringComparison.Ordinal))
        {
            yield break;
        }

        foreach (AnalysisItem item in EvaluateSTXE001(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXE002(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXE003(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXE004(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXE005(context: context))
        {
            yield return item;
        }
    }

    private static AnalysisItem CreateAnalysisItem(
        string code,
        string description,
        EvaluationContext context,
        Microsoft.CodeAnalysis.Location? location = null
    )
    {
        return new AnalysisItem
        {
            Code = code,
            Description = description,
            Severity = AnalysisSeverity.Warning,
            Type = context.TypeName,
            LineNumber = location is null ? context.LineNumber : location.GetLineSpan().StartLinePosition.Line + 1,
        };
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXE001(EvaluationContext context)
    {
        return context.IsApiController || context.TypeName.Split(separator: ['.'])
            .Last() == "Program"
            ? []
            : context
                .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.DescendantNodes())
            .Where(
                    predicate: (Microsoft.CodeAnalysis.SyntaxNode node) =>
                        node is IfStatementSyntax or SwitchStatementSyntax or ConditionalExpressionSyntax
                )
                .Where(
                    predicate: (Microsoft.CodeAnalysis.SyntaxNode node) =>
                        !IsMvcActionResponseNode(node: node))
                .Select(
                    selector: (Microsoft.CodeAnalysis.SyntaxNode node) =>
                        CreateAnalysisItem(
                            code: "STXE001",
                            description: "An exposure must not contain branching logic.",
                            context: context,
                            location: node.GetLocation()
                        )
                );
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXE002(EvaluationContext context) =>

        context
            .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.DescendantNodes())
            .Where(
                predicate: (Microsoft.CodeAnalysis.SyntaxNode node) =>
                    node is ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax or DoStatementSyntax
            )
            .Select(
                selector: (Microsoft.CodeAnalysis.SyntaxNode node) =>
                    CreateAnalysisItem(
                        code: "STXE002",
                        description: "An exposure must not contain loops.",
                        context: context,
                        location: node.GetLocation()
                    )
            );

    private static IEnumerable<AnalysisItem> EvaluateSTXE003(EvaluationContext context)
    {
        if (context.IsApiController)
        {
            return [];
        }

        int serviceDependencyCount = context.Dependencies.Count(
            predicate: (TypeDependency dependency) =>
                dependency.StandardElementType
                    is >= StandardElementType.FoundationService
                        and <= StandardElementType.AggregationService
        );

        return serviceDependencyCount <= 1
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXE003",
                    description: "An exposure may communicate with only one business service.",
                    context: context
                ),
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXE004(EvaluationContext context)
    {
        return !context.Dependencies.Any(
            predicate: (TypeDependency dependency) => dependency.StandardElementType == StandardElementType.Broker
        )
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXE004",
                    description: "An exposure must not communicate directly with a broker.",
                    context: context
                ),
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXE005(EvaluationContext context)
    {
        return context.IsApiController || context.TypeName.Split(separator: ['.'])
            .Last() == "Program"
            ? []
            : context
                .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
                .Where(
                    predicate: (MethodDeclarationSyntax method) =>
                        method.Body is not null
                        && !IsMvcActionResponseMethod(method: method)
                        && method.Body.Statements.Count(
                            predicate: (StatementSyntax statement) =>
                                statement.DescendantNodesAndSelf()
                                    .OfType<InvocationExpressionSyntax>()
                                    .Any()
                        ) > 1
                )
                .Select(
                    selector: (MethodDeclarationSyntax method) =>
                        CreateAnalysisItem(
                            code: "STXE005",
                            description: "An exposure must not sequence multiple routine calls.",
                            context: context,
                            location: method.GetLocation()
                        )
                );
    }

    private static bool IsMvcActionResponseNode(
        Microsoft.CodeAnalysis.SyntaxNode node)
    {
        MethodDeclarationSyntax? method = node
            .Ancestors()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault();

        return method is not null
            && IsMvcActionResponseMethod(method: method);
    }

    private static bool IsMvcActionResponseMethod(
        MethodDeclarationSyntax method) =>
        method.Modifiers.Any(
            predicate: modifier =>
                modifier.RawKind == (int)SyntaxKind.PublicKeyword)
        && method.ReturnType
            .ToString()
            .Contains(
                value: "IActionResult",
                comparisonType: StringComparison.Ordinal);
}
