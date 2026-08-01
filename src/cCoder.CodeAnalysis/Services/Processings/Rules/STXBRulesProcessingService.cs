// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXBRulesProcessingService : ISTXBRulesProcessingService
{
    private static readonly IArchitectureModelQueriesProcessingService architectureModelQueries =
        new ArchitectureModelQueriesProcessingService();

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateSTXB001(context: context)
            .Concat(second: EvaluateSTXB002(context: context))
            .Concat(second: EvaluateSTXB003(context: context))
            .Concat(second: EvaluateSTXB004(context: context))
            .Concat(second: EvaluateSTXB005(context: context))
            .Concat(second: EvaluateSTXB006(context: context))
            .Concat(second: EvaluateSTXB007(context: context));
    }

    private IEnumerable<AnalysisItem> EvaluateSTXB001(EvaluationContext context)
    {
        if (IsUtilityBroker(context: context))
        {
            return Array.Empty<AnalysisItem>();
        }

        int dependencyCount = architectureModelQueries.GetDependencies(context: context).Count(
            predicate: delegate (TypeDependency dependency)
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
                    code: "STXB001",
                    description: "A broker may have at most one external or exposure dependency.",
                    context: context
                ),
            };
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXB002(EvaluationContext context) =>

        architectureModelQueries.GetDeclarations(context: context)
            .SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.DescendantNodes())
            .Where(
                predicate: (SyntaxNode node) =>
                    node is IfStatementSyntax or SwitchStatementSyntax or ConditionalExpressionSyntax
            )
            .Select(
                selector: (SyntaxNode node) =>
                    CreateAnalysisItem(
                        code: "STXB002",
                        description: "A broker must not contain branching logic.",
                        context: context,
                        lineNumber: node.GetLocation()
            .GetLineSpan().StartLinePosition.Line + 1
                    )
            );

    private static IEnumerable<AnalysisItem> EvaluateSTXB003(EvaluationContext context) =>

        architectureModelQueries.GetDeclarations(context: context)
            .SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.DescendantNodes())
            .Where(
                predicate: (SyntaxNode node) =>
                    node is ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax or DoStatementSyntax
            )
            .Select(
                selector: (SyntaxNode node) =>
                    CreateAnalysisItem(
                        code: "STXB003",
                        description: "A broker must not contain loops.",
                        context: context,
                        lineNumber: node.GetLocation()
            .GetLineSpan().StartLinePosition.Line + 1
                    )
            );

    private IEnumerable<AnalysisItem> EvaluateSTXB004(EvaluationContext context)
    {
        return architectureModelQueries.ImplementsContract(context: context)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: "STXB004",
                    description: "A broker must implement a local interface.",
                    context: context
                ),
            };
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXB005(EvaluationContext context) =>

        architectureModelQueries.GetDeclarations(context: context)
            .SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.DescendantNodes())
            .OfType<TryStatementSyntax>()
            .Select(
                selector: (TryStatementSyntax node) =>
                    CreateAnalysisItem(
                        code: "STXB005",
                        description: "A broker must not handle exceptions.",
                        context: context,
                        lineNumber: node.GetLocation()
            .GetLineSpan().StartLinePosition.Line + 1
                    )
            );

    private IEnumerable<AnalysisItem> EvaluateSTXB006(EvaluationContext context)
    {
        if (IsUtilityBroker(context: context))
        {
            return Array.Empty<AnalysisItem>();
        }

        return (
            !architectureModelQueries.GetDependencies(context: context).Any(
                predicate: delegate (TypeDependency dependency)
                {
                    StandardElementType standardElementType = dependency.StandardElementType;

                    return standardElementType != StandardElementType.Dependency
                        && standardElementType != StandardElementType.Exposure
                        && !IsConfigurationDependency(dependency: dependency);
                }
            )
        )
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: "STXB006",
                    description: "A broker must only depend on an external dependency or exposure.",
                    context: context
                ),
            };
    }

    private static bool IsConfigurationDependency(TypeDependency dependency) =>
        dependency.StandardElementType == StandardElementType.Model
        && dependency.TypeName.EndsWith(
            value: "Configuration",
            comparisonType: StringComparison.Ordinal);

    private static bool IsUtilityBroker(EvaluationContext context)
    {
        string typeName = architectureModelQueries.GetTypeName(
            context: context);

        return typeName.Contains(
                value: ".Brokers.Utility.",
                comparisonType: StringComparison.Ordinal)
            && typeName.EndsWith(
                value: "UtilityBroker",
                comparisonType: StringComparison.Ordinal);
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXB007(EvaluationContext context)
    {
        if (!architectureModelQueries.GetTypeName(context: context).Contains(value: ".Brokers.Storage.", comparisonType: StringComparison.Ordinal))
        {
            return Array.Empty<AnalysisItem>();
        }

        string[] verbs = new string[4] { "Select", "Insert", "Update", "Delete" };

        return architectureModelQueries.GetDeclarations(context: context)
            .SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .Where(
                predicate: (MethodDeclarationSyntax method) =>
                    method.Modifiers.Any(predicate: (SyntaxToken token) => token.RawKind == 8343)
            )
            .Where(
                predicate: (MethodDeclarationSyntax method) =>
                    !verbs.Any(
                        predicate: (string verb) =>
                            method.Identifier.Text.StartsWith(value: verb, comparisonType: StringComparison.Ordinal)
                    )
            )
            .Select(
                selector: (MethodDeclarationSyntax method) =>
                    CreateAnalysisItem(
                        code: "STXB007",
                        description: "Storage broker methods must use SQL nouns: Select, Insert, Update, or Delete.",
                        context: context,
                        lineNumber: method.GetLocation()
            .GetLineSpan().StartLinePosition.Line + 1
                    )
            );
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
            Type = architectureModelQueries.GetTypeName(context: context),
            LineNumber = (lineNumber ?? architectureModelQueries.GetLineNumber(context: context)),
        };
    }
}