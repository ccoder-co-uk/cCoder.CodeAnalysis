// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXEXRulesProcessingService : ISTXEXRulesProcessingService
{
    private static readonly IArchitectureModelQueriesProcessingService architectureModelQueries =
        new ArchitectureModelQueriesProcessingService();

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        if (
            architectureModelQueries.GetImplementedInterfaces(context).Any(
                predicate: (string interfaceName) =>
                    interfaceName.EndsWith(value: ".IRuleProcessingService", comparisonType: StringComparison.Ordinal)
                    || interfaceName.EndsWith(
                        value: ".ICodeAnalysisInfrastructureService",
                        comparisonType: StringComparison.Ordinal
                    )
            ) == true
        )
        {
            return [];
        }

        return EvaluateSTXEX001(context: context)
            .Concat(second: EvaluateSTXEX002(context: context))
            .Concat(second: EvaluateSTXEX003(context: context));
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXEX001(EvaluationContext context) =>

        EvaluateExceptionCategory(
            context: context,
            categoryName: "Validation",
            code: "STXEX001",
            description: "Every TryCatch overload must classify, wrap, and preserve validation exceptions."
        );

    private static IEnumerable<AnalysisItem> EvaluateSTXEX002(EvaluationContext context) =>

        EvaluateExceptionCategory(
            context: context,
            categoryName: "Dependency",
            code: "STXEX002",
            description: "Every TryCatch overload must classify, wrap, and preserve dependency exceptions."
        );

    private static IEnumerable<AnalysisItem> EvaluateSTXEX003(EvaluationContext context)
    {
        MethodDeclarationSyntax[] tryCatchMethods = GetTryCatchMethods(context: context);

        bool wrapsDefaultExceptions =
            tryCatchMethods.Length > 0
            && tryCatchMethods.All(
                predicate: (MethodDeclarationSyntax method) =>
                    method
                        .DescendantNodes()
            .OfType<CatchClauseSyntax>()
                        .Any(
                            predicate: (CatchClauseSyntax catchClause) =>
                                catchClause.Declaration?.Type.ToString() == "Exception"
                                && CatchWrapsException(catchClause: catchClause, categoryName: null)
                        )
            );

        return wrapsDefaultExceptions
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXEX003",
                    description: "Every TryCatch overload must wrap and preserve unclassified exceptions.",
                    context: context
                ),
            ];
    }

    private static AnalysisItem[] EvaluateExceptionCategory(
        EvaluationContext context,
        string categoryName,
        string code,
        string description
    )
    {
        MethodDeclarationSyntax[] tryCatchMethods = GetTryCatchMethods(context: context);

        bool wrapsCategory =
            tryCatchMethods.Length > 0
            && tryCatchMethods.All(
                predicate: (MethodDeclarationSyntax method) =>
                    method
                        .DescendantNodes()
            .OfType<CatchClauseSyntax>()
                        .Any(
                            predicate: (CatchClauseSyntax catchClause) =>
                                CatchWrapsException(catchClause: catchClause, categoryName: categoryName)
                        )
            );

        return wrapsCategory ? [] : [CreateAnalysisItem(code: code, description: description, context: context)];
    }

    private static MethodDeclarationSyntax[] GetTryCatchMethods(EvaluationContext context) =>

        architectureModelQueries
            .GetDeclarations(context).Where(
                predicate: (TypeDeclarationSyntax declaration) =>
                    declaration.SyntaxTree.FilePath.EndsWith(
                        value: ".Exceptions.cs",
                        comparisonType: StringComparison.Ordinal
                    )
            )
            .SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .Where(predicate: (MethodDeclarationSyntax method) => method.Identifier.Text == "TryCatch")
            .ToArray();

    private static bool CatchWrapsException(CatchClauseSyntax catchClause, string? categoryName)
    {
        string caughtExceptionName = catchClause.Declaration?.Identifier.Text ?? string.Empty;

        return caughtExceptionName.Length > 0
            && catchClause
                .Block.DescendantNodes()
            .OfType<ThrowStatementSyntax>()
                .Select(selector: (ThrowStatementSyntax statement) => statement.Expression)
                .OfType<ObjectCreationExpressionSyntax>()
                .Any(
                    predicate: (ObjectCreationExpressionSyntax objectCreation) =>
                        (
                            categoryName is null
                            || objectCreation
                                .Type.ToString()
            .Contains(value: categoryName, comparisonType: StringComparison.Ordinal)
                        )
                        && objectCreation.ArgumentList?.Arguments.Any(
                            predicate: (ArgumentSyntax argument) =>
                                argument.Expression.ToString() == caughtExceptionName
                        ) == true
                );
    }

    private static AnalysisItem CreateAnalysisItem(string code, string description, EvaluationContext context)
    {
        return new AnalysisItem
        {
            Code = code,
            Description = description,
            Severity = AnalysisSeverity.Warning,
            Type = architectureModelQueries.GetTypeName(context),
            LineNumber = architectureModelQueries.GetLineNumber(context),
        };
    }
}
