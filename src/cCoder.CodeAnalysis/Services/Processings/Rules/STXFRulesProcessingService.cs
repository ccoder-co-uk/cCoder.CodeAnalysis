// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXFRulesProcessingService : ISTXFRulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        foreach (AnalysisItem item in EvaluateSTXF001(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXF002(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXF003(context: context))
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

    private static IEnumerable<AnalysisItem> EvaluateSTXF001(EvaluationContext context) =>

        context
            .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.DescendantNodes())
            .Where(
                predicate: (SyntaxNode node) =>
                    (node is ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax or DoStatementSyntax)
                    && !node.SyntaxTree.FilePath.EndsWith(
                        value: ".Validations.cs",
                        comparisonType: StringComparison.Ordinal
                    )
                    && context.PublicApiModelTypes.Any(
                        predicate: (string modelType) =>
                            node.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
                                .Any(
                                    predicate: (IdentifierNameSyntax identifier) =>
                                        identifier.Identifier.Text
                                        == modelType.Substring(startIndex: modelType.LastIndexOf(value: '.') + 1)
                                )
                    )
            )
            .Select(
                selector: (SyntaxNode node) =>
                    new AnalysisItem
                    {
                        Code = "STXF001",
                        Description = "A foundation service must not loop over its service model type.",
                        Severity = AnalysisSeverity.Warning,
                        Type = context.TypeName,
                        LineNumber = node.GetLocation()
            .GetLineSpan().StartLinePosition.Line + 1,
                    }
            );

    private static IEnumerable<AnalysisItem> EvaluateSTXF002(EvaluationContext context)
    {
        return (
            !context.Dependencies.Any(
                predicate: delegate (TypeDependency dependency)
                {
                    StandardElementType standardElementType = dependency.StandardElementType;

                    return standardElementType != StandardElementType.Broker
                        && standardElementType != StandardElementType.Exposure
                        && !IsLoggingDependency(dependency: dependency);
                }
            )
        )
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                new AnalysisItem
                {
                    Code = "STXF002",
                    Description = "A foundation service may only depend on brokers, exposures, or nothing.",
                    Severity = AnalysisSeverity.Warning,
                    Type = context.TypeName,
                    LineNumber = context.LineNumber,
                },
            };
    }

    private static bool IsLoggingDependency(TypeDependency dependency) =>
        dependency.TypeName.StartsWith(
            value: "Microsoft.Extensions.Logging.ILogger",
            comparisonType: StringComparison.Ordinal);

    private static IEnumerable<AnalysisItem> EvaluateSTXF003(EvaluationContext context) =>

        context
            .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .SelectMany(
                selector: (MethodDeclarationSyntax method) =>
                    method
                        .ParameterList.Parameters.Where(
                            predicate: (ParameterSyntax parameter) =>
                                context.PublicApiModelTypes.Any(
                                    predicate: (string modelType) =>
                                        modelType.EndsWith(
                                            value: $".{parameter.Type}",
                                            comparisonType: StringComparison.Ordinal
                                        )
                                )
                        )
            .SelectMany(
                            selector: (ParameterSyntax parameter) =>
                                method
                                    .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
                                    .Where(
                                        predicate: (InvocationExpressionSyntax invocation) =>
                                            invocation.Expression
                                                is MemberAccessExpressionSyntax memberAccessExpressionSyntax
                                            && (
                                                memberAccessExpressionSyntax.Name.Identifier.Text.StartsWith(
                                                    value: "Insert",
                                                    comparisonType: StringComparison.Ordinal
                                                )
                                                || memberAccessExpressionSyntax.Name.Identifier.Text.StartsWith(
                                                    value: "Update",
                                                    comparisonType: StringComparison.Ordinal
                                                )
                                            )
                                    )
                                    .Where(
                                        predicate: (InvocationExpressionSyntax invocation) =>
                                            invocation.ArgumentList.Arguments.Any(
                                                predicate: (ArgumentSyntax argument) =>
                                                    argument.Expression
                                                        is IdentifierNameSyntax { Identifier: var identifier }
                                                    && identifier.Text == parameter.Identifier.Text
                                            )
                                    )
                                    .Select(
                                        selector: (InvocationExpressionSyntax invocation) =>
                                            CreateAnalysisItem(
                                                code: "STXF003",
                                                description: "Foundation mutations must pass a flat single-row model to their broker.",
                                                context: context,
                                                location: invocation.GetLocation()
                                            )
                                    )
                        )
            );
}