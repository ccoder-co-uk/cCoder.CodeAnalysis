// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXARulesProcessingService : ISTXARulesProcessingService
{
    private static readonly IArchitectureModelQueriesProcessingService architectureModelQueries =
        new ArchitectureModelQueriesProcessingService();

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateSTXA001(context: context)
            .Concat(second: EvaluateSTXA002(context: context))
            .Concat(second: EvaluateSTXA003(context: context));
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
            Type = architectureModelQueries.GetTypeName(context: context),
            FilePath = location?.SourceTree?.FilePath,
            LineNumber = location is null ? architectureModelQueries.GetLineNumber(context: context) : location.GetLineSpan().StartLinePosition.Line + 1,
        };
    }

    private IEnumerable<AnalysisItem> EvaluateSTXA001(EvaluationContext context)
    {
        bool hasSingleDependencyVariation =
            architectureModelQueries
                .GetDependencies(context: context)
                .Where(predicate: (TypeDependency dependency) =>
                    IsServiceVariation(
                        standardElementType: dependency.StandardElementType))
                .Select(selector: (TypeDependency dependency) =>
                    dependency.StandardElementType)
                .Distinct()
                .Count() <= 1;

        return hasSingleDependencyVariation
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: "STXA001",
                    description: "An aggregation service's business dependencies must share the same service variation.",
                    context: context
                ),
            };
    }

    private static bool IsServiceVariation(
        StandardElementType standardElementType) =>
        standardElementType is
            StandardElementType.FoundationService or
            StandardElementType.ProcessingService or
            StandardElementType.OrchestrationService or
            StandardElementType.CoordinationService or
            StandardElementType.ManagementService or
            StandardElementType.AggregationService;

    private static IEnumerable<AnalysisItem> EvaluateSTXA002(EvaluationContext context)
    {
        string typeName = architectureModelQueries.GetTypeName(context: context).Split(separator: ['.'])
            .Last();

        return typeName.Contains(value: "Aggregation", comparisonType: StringComparison.Ordinal)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: "STXA002",
                    description: "An aggregation service name must contain the Aggregation identifier.",
                    context: context
                ),
            };
    }

    private IEnumerable<AnalysisItem> EvaluateSTXA003(EvaluationContext context)
    {
        int businessServiceDependencyCount = architectureModelQueries
            .GetDependencies(context: context)
            .Count(predicate: dependency =>
                IsServiceVariation(
                    standardElementType: dependency.StandardElementType)
                || IsCompositionExposureDependency(
                    context: context,
                    dependency: dependency));

        bool containsOnlyPassThroughMethods = ContainsOnlyPassThroughMethods(
            context: context);

        return businessServiceDependencyCount >= 2
            || !containsOnlyPassThroughMethods
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXA003",
                    description: "An aggregation service with fewer than two business dependencies must add aggregation behavior; a pass-through wrapper is redundant.",
                    context: context)
            ];
    }

    private static bool ContainsOnlyPassThroughMethods(
        EvaluationContext context)
    {
        MethodDeclarationSyntax[] methods = architectureModelQueries
            .GetDeclarations(context: context)
            .Where(declaration =>
                !declaration.SyntaxTree.FilePath.EndsWith(
                    value: ".Validations.cs",
                    comparisonType: StringComparison.Ordinal)
                && !declaration.SyntaxTree.FilePath.EndsWith(
                    value: ".Exceptions.cs",
                    comparisonType: StringComparison.Ordinal))
            .SelectMany(declaration => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .ToArray();

        return methods.Length == 0
            || methods.All(predicate: IsSinglePassThroughMethod);
    }

    private static bool IsSinglePassThroughMethod(
        MethodDeclarationSyntax method)
    {
        LambdaExpressionSyntax? tryCatchLambda = method
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault(invocation =>
                invocation.Expression.ToString() == "TryCatch")?
            .ArgumentList.Arguments.FirstOrDefault()?
            .Expression as LambdaExpressionSyntax;

        if (tryCatchLambda?.Body is ExpressionSyntax lambdaExpression)
        {
            return IsInvocation(expression: lambdaExpression);
        }

        if (tryCatchLambda?.Body is BlockSyntax lambdaBlock)
        {
            StatementSyntax[] businessStatements = lambdaBlock.Statements
                .Where(statement => !statement.ToString().StartsWith(
                    value: "Validate(",
                    comparisonType: StringComparison.Ordinal))
                .ToArray();

            return businessStatements.Length == 1
                && IsInvocationStatement(statement: businessStatements[0]);
        }

        if (method.ExpressionBody is not null)
        {
            return IsInvocation(expression: method.ExpressionBody.Expression);
        }

        return method.Body?.Statements.Count == 1
            && IsInvocationStatement(statement: method.Body.Statements[index: 0]);
    }

    private static bool IsInvocationStatement(StatementSyntax statement) =>
        statement switch
        {
            ReturnStatementSyntax { Expression: not null } returnStatement =>
                IsInvocation(expression: returnStatement.Expression),
            ExpressionStatementSyntax expressionStatement =>
                IsInvocation(expression: expressionStatement.Expression),
            _ => false,
        };

    private static bool IsInvocation(ExpressionSyntax expression)
    {
        if (expression is AwaitExpressionSyntax awaitExpression)
        {
            return IsInvocation(expression: awaitExpression.Expression);
        }

        return expression is InvocationExpressionSyntax;
    }

    private static bool IsCompositionExposureDependency(
        EvaluationContext context,
        TypeDependency dependency)
    {
        string dependencyTypeName = dependency.TypeName?.Split(separator: ['.']).Last() ?? string.Empty;

        return context.ArchitectureModel.Classes.Any(candidate =>
            candidate.StandardElementType == StandardElementType.Exposure
            && candidate.AnalysisDirectlyImplementedInterfaces?.Any(
                interfaceName => interfaceName.EndsWith(
                    value: ".Exposures.ICompositionExposure",
                    comparisonType: StringComparison.Ordinal)) == true
            && candidate.Interfaces.Any(contract =>
                string.Equals(
                    a: contract.FullName,
                    b: dependency.TypeName,
                    comparisonType: StringComparison.Ordinal)
                || string.Equals(
                    a: contract.Name,
                    b: dependencyTypeName,
                    comparisonType: StringComparison.Ordinal)));
    }
}