// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class BrokerCodeAnalysisRulesProcessingService
    : CodeAnalysisRulesProcessingService,
        IBrokerCodeAnalysisRulesProcessingService
{
    public AnalysisItem[] Evaluate(EvaluationContext context)
    {
        List<AnalysisItem> list = new List<AnalysisItem>();
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateSourceFormatting(context));
        list.AddRange(BrokerCodeAnalysisRulesProcessingService.EvaluateSTXB001(context));
        list.AddRange(BrokerCodeAnalysisRulesProcessingService.EvaluateSTXB002(context));
        list.AddRange(BrokerCodeAnalysisRulesProcessingService.EvaluateSTXB003(context));
        list.AddRange(BrokerCodeAnalysisRulesProcessingService.EvaluateSTXB004(context));
        list.AddRange(BrokerCodeAnalysisRulesProcessingService.EvaluateSTXB005(context));
        list.AddRange(BrokerCodeAnalysisRulesProcessingService.EvaluateSTXB006(context));
        list.AddRange(BrokerCodeAnalysisRulesProcessingService.EvaluateSTXB007(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluatePropertiesAreNotAllowed(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateBusinessImplementationVisibility(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateTypedIdentifiers(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateMutationNaming(context));
        return list.ToArray();
    }

    private static AnalysisItem[] EvaluateSTXB001(EvaluationContext context)
    {
        int dependencyCount = context.Dependencies.Count(
            delegate(TypeDependency dependency)
            {
                StandardElementType standardElementType = dependency.StandardElementType;
                return (
                    standardElementType == StandardElementType.Exposure
                    || standardElementType == StandardElementType.Dependency
                )
                    ? true
                    : false;
            }
        );
        return (dependencyCount <= 1)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    "STXB001",
                    "A broker may have at most one external or exposure dependency.",
                    context
                ),
            };
    }

    private static AnalysisItem[] EvaluateSTXB002(EvaluationContext context)
    {
        return (
            from node in context.Declarations.SelectMany(
                (TypeDeclarationSyntax declaration) => declaration.DescendantNodes()
            )
            where
                (node is IfStatementSyntax || node is SwitchStatementSyntax || node is ConditionalExpressionSyntax)
                    ? true
                    : false
            select CreateAnalysisItem(
                "STXB002",
                "A broker must not contain branching logic.",
                context,
                node.GetLocation().GetLineSpan().StartLinePosition.Line + 1
            )
        ).ToArray();
    }

    private static AnalysisItem[] EvaluateSTXB003(EvaluationContext context)
    {
        return (
            from node in context.Declarations.SelectMany(
                (TypeDeclarationSyntax declaration) => declaration.DescendantNodes()
            )
            where
                (
                    node is ForStatementSyntax
                    || node is ForEachStatementSyntax
                    || node is WhileStatementSyntax
                    || node is DoStatementSyntax
                )
                    ? true
                    : false
            select CreateAnalysisItem(
                "STXB003",
                "A broker must not contain loops.",
                context,
                node.GetLocation().GetLineSpan().StartLinePosition.Line + 1
            )
        ).ToArray();
    }

    private static AnalysisItem[] EvaluateSTXB004(EvaluationContext context)
    {
        return (context.ImplementedInterfaces.Count != 0)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem("STXB004", "A broker must implement a local interface.", context),
            };
    }

    private static AnalysisItem[] EvaluateSTXB005(EvaluationContext context)
    {
        return (
            from node in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.DescendantNodes())
                .OfType<TryStatementSyntax>()
            select CreateAnalysisItem(
                "STXB005",
                "A broker must not handle exceptions.",
                context,
                node.GetLocation().GetLineSpan().StartLinePosition.Line + 1
            )
        ).ToArray();
    }

    private static AnalysisItem[] EvaluateSTXB006(EvaluationContext context)
    {
        return (
            !context.Dependencies.Any(
                delegate(TypeDependency dependency)
                {
                    StandardElementType standardElementType = dependency.StandardElementType;
                    return standardElementType != StandardElementType.Dependency
                        && standardElementType != StandardElementType.Exposure;
                }
            )
        )
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    "STXB006",
                    "A broker must only depend on an external dependency or exposure.",
                    context
                ),
            };
    }

    private static AnalysisItem[] EvaluateSTXB007(EvaluationContext context)
    {
        if (!context.TypeName.Contains(".Brokers.Storage.", StringComparison.Ordinal))
        {
            return Array.Empty<AnalysisItem>();
        }
        string[] verbs = new string[4] { "Select", "Insert", "Update", "Delete" };
        return (
            from method in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                .OfType<MethodDeclarationSyntax>()
            where method.Modifiers.Any((SyntaxToken token) => token.RawKind == 8343)
            where !verbs.Any((string verb) => method.Identifier.Text.StartsWith(verb, StringComparison.Ordinal))
            select CreateAnalysisItem(
                "STXB007",
                "Storage broker methods must use SQL nouns: Select, Insert, Update, or Delete.",
                context,
                method.GetLocation().GetLineSpan().StartLinePosition.Line + 1
            )
        ).ToArray();
    }

    private static AnalysisItem CreateAnalysisItem(
        string code,
        string description,
        EvaluationContext context,
        int? lineNumber = null
    )
    {
        return new AnalysisItem
        {
            Code = code,
            Description = description,
            Severity = AnalysisSeverity.Warning,
            Type = context.TypeName,
            LineNumber = (lineNumber ?? context.LineNumber),
        };
    }
}