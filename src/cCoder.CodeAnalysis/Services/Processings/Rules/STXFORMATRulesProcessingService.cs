// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXFORMATRulesProcessingService : ISTXFORMATRulesProcessingService
{
    private static readonly IArchitectureModelQueriesProcessingService
        architectureModelQueries = new ArchitectureModelQueriesProcessingService();
    private static AnalysisItem CreateAnalysisItem(
        string code,
        string description,
        EvaluationContext context,
        Location? location = null
    )
    {
        return new AnalysisItem
        {
            Code = code,
            Description = description,
            Severity = AnalysisSeverity.Warning,
            Type = context.TypeName,
            LineNumber = location is not null ? location.GetLineSpan().StartLinePosition.Line + 1 : context.LineNumber,
        };
    }

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateSTXFORMAT011(context: context)
            .Concat(second: EvaluateSTXFORMAT001(context: context))
            .Concat(second: EvaluateSTXFORMAT002(context: context))
            .Concat(second: EvaluateSTXFORMAT003(context: context))
            .Concat(second: EvaluateSTXFORMAT008(context: context))
            .Concat(second: EvaluateSTXFORMAT004(context: context))
            .Concat(second: EvaluateSTXFORMAT005(context: context))
            .Concat(second: EvaluateSTXFORMAT006(context: context))
            .Concat(second: EvaluateSTXFORMAT007(context: context))
            .Concat(second: EvaluateSTXFORMAT009(context: context))
            .Concat(second: EvaluateSTXFORMAT010(context: context))
            .Concat(second: EvaluateSTXFORMAT012(context: context))
            .Concat(second: EvaluateSTXFORMAT013(context: context));
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXFORMAT001(EvaluationContext context) =>

        architectureModelQueries
            .GetDeclarations(context: context).GroupBy<TypeDeclarationSyntax, string>(
                keySelector: (TypeDeclarationSyntax declaration) => declaration.SyntaxTree.FilePath,
                comparer: StringComparer.OrdinalIgnoreCase
            )
            .Select(selector: (IGrouping<string, TypeDeclarationSyntax> declarations) => declarations.First())
            .Where(
                predicate: (TypeDeclarationSyntax declaration) =>
                    HasTrailingNewlines(source: declaration.SyntaxTree.GetText()
            .ToString())
            )
            .Select(
                selector: (TypeDeclarationSyntax declaration) =>
                    CreateAnalysisItem(
                        code: "STXFORMAT001",
                        description: "Source files must not contain newline characters after the final code token.",
                        context: context,
                        location: declaration
                            .SyntaxTree.GetRoot()
                            .GetLastToken()
                            .GetLocation()
                    )
            );

    private static IEnumerable<AnalysisItem> EvaluateSTXFORMAT002(EvaluationContext context) =>

        architectureModelQueries
            .GetDeclarations(context: context).SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .Where(predicate: (MethodDeclarationSyntax method) => method.ExpressionBody != null)
            .Where(
                predicate: delegate (MethodDeclarationSyntax method)
                {
                    LinePosition startLinePosition = method.GetLocation()
                        .GetLineSpan().StartLinePosition;

                    LinePosition startLinePosition2 = method
                        .ExpressionBody!.ArrowToken.GetLocation()
                        .GetLineSpan()
                        .StartLinePosition;

                    FileLinePositionSpan lineSpan = method.ExpressionBody.Expression.GetLocation()
                        .GetLineSpan();

                    int num = startLinePosition.Character + 4;

                    return startLinePosition2.Line == lineSpan.StartLinePosition.Line
                        || lineSpan.StartLinePosition.Character != num;
                }
            )
            .Select(
                selector: (MethodDeclarationSyntax method) =>
                    CreateAnalysisItem(
                        code: "STXFORMAT002",
                        description: "Expression-bodied method implementations must start on the line after the => token.",
                        context: context,
                        location: method.ExpressionBody!.ArrowToken.GetLocation()
                    )
            );

    private static IEnumerable<AnalysisItem> EvaluateSTXFORMAT003(EvaluationContext context) =>

        architectureModelQueries
            .GetDeclarations(context: context).SelectMany(
                selector: (TypeDeclarationSyntax declaration) =>
                    declaration
                        .DescendantNodes()
                        .OfType<BlockSyntax>()
            )
            .Where(
                predicate: (BlockSyntax block) =>
                    block
                        .Ancestors()
                        .OfType<MethodDeclarationSyntax>()
                        .Any()
            )
            .SelectMany(
                selector: (BlockSyntax block) =>
                    block.Statements.Select(
                        selector: (StatementSyntax statement, int index) =>
                            new
                            {
                                Statement = statement,
                                Previous = ((index > 0) ? block.Statements[index: index - 1] : null),
                                Next = (
                                    (index < block.Statements.Count - 1) ? block.Statements[index: index + 1] : null
                                ),
                            }
                    )
            )
            .Where(predicate: item => IsControlFlowStatement(statement: item.Statement))
            .Where(predicate: item =>
                (item.Previous != null && !HasBlankLineBetween(first: item.Previous, second: item.Statement))
                || (item.Next != null && !HasBlankLineBetween(first: item.Statement, second: item.Next))
            )
            .Select(selector: item =>
                CreateAnalysisItem(
                    code: "STXFORMAT003",
                    description: "Control-flow blocks must be separated from adjacent method statements by an empty line.",
                    context: context,
                    location: item.Statement.GetLocation()
                )
            );

    private static IEnumerable<AnalysisItem> EvaluateSTXFORMAT004(EvaluationContext context) =>

        architectureModelQueries
            .GetDeclarations(context: context).SelectMany(
                selector: (TypeDeclarationSyntax declaration) =>
                    declaration.Members.Zip(
                        second: declaration.Members.Skip(count: 1),
                        resultSelector: (MemberDeclarationSyntax first, MemberDeclarationSyntax second) =>
                            new { First = first, Second = second }
                    )
            )
            .Where(predicate: pair => pair.First is MethodDeclarationSyntax && pair.Second is MethodDeclarationSyntax)
            .Where(predicate: pair => !HasExactlyOneBlankLineBetween(first: pair.First, second: pair.Second))
            .Select(selector: pair =>
                CreateAnalysisItem(
                    code: "STXFORMAT004",
                    description: "Adjacent methods must have exactly one empty line between them.",
                    context: context,
                    location: pair.Second.GetLocation()
                )
            );

    private static IEnumerable<AnalysisItem> EvaluateSTXFORMAT005(EvaluationContext context) =>

        architectureModelQueries
            .GetDeclarations(context: context).SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.DescendantNodes())
            .OfType<InvocationExpressionSyntax>()
            .Where(
                predicate: (InvocationExpressionSyntax invocation) =>
                    invocation.Expression.ToString() != "nameof"
                    && invocation.ArgumentList.Arguments.Any(
                        predicate: (ArgumentSyntax argument) => argument.NameColon == null
                    )
            )
            .Select(
                selector: (InvocationExpressionSyntax invocation) =>
                    CreateAnalysisItem(
                        code: "STXFORMAT005",
                        description: "Method call arguments must declare their parameter names.",
                        context: context,
                        location: invocation.GetLocation()
                    )
            );

    private static IEnumerable<AnalysisItem> EvaluateSTXFORMAT006(EvaluationContext context) =>

        architectureModelQueries
            .GetDeclarations(context: context).SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .Where(predicate: (MethodDeclarationSyntax method) => GetSingleInvocation(method: method) != null)
            .Where(
                predicate: delegate (MethodDeclarationSyntax method)
                {
                    InvocationExpressionSyntax singleInvocation = GetSingleInvocation(method: method)!;

                    FileLinePositionSpan lineSpan = singleInvocation.GetLocation()
                        .GetLineSpan();

                    return lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line;
                }
            )
            .Select(
                selector: (MethodDeclarationSyntax method) =>
                    CreateAnalysisItem(
                        code: "STXFORMAT006",
                        description: "A single multiline method call must be expressed as an expression-bodied method.",
                        context: context,
                        location: method.GetLocation()
                    )
            );

    private static IEnumerable<AnalysisItem> EvaluateSTXFORMAT007(EvaluationContext context) =>

        architectureModelQueries
            .GetDeclarations(context: context).SelectMany(
                selector: (TypeDeclarationSyntax declaration) => declaration.DescendantNodes()
            .OfType<StatementSyntax>()
            )
            .Where(predicate: RequiresScopeBraces)
            .Select(
                selector: (StatementSyntax statement) =>
                    CreateAnalysisItem(
                        code: "STXFORMAT007",
                        description: "Control-flow statement bodies must always be enclosed in scope braces.",
                        context: context,
                        location: statement.GetLocation()
                    )
            );

    private static IEnumerable<AnalysisItem> EvaluateSTXFORMAT008(EvaluationContext context) =>

        architectureModelQueries
            .GetDeclarations(context: context).SelectMany(
                selector: (TypeDeclarationSyntax declaration) =>
                    declaration
                        .DescendantNodes()
                        .OfType<BlockSyntax>()
            )
            .Where(
                predicate: (BlockSyntax block) =>
                    block
                        .Ancestors()
                        .OfType<MethodDeclarationSyntax>()
                        .Any()
            )
            .SelectMany(
                selector: (BlockSyntax block) =>
                    block.Statements.Select(
                        selector: (StatementSyntax statement, int index) =>
                            new
                            {
                                Statement = statement,
                                Previous = ((index > 0) ? block.Statements[index: index - 1] : null),
                                Next = (
                                    (index < block.Statements.Count - 1) ? block.Statements[index: index + 1] : null
                                ),
                            }
                    )
            )
            .Where(predicate: item =>
                item.Statement.GetLocation()
            .GetLineSpan().StartLinePosition.Line
                != item.Statement.GetLocation()
            .GetLineSpan().EndLinePosition.Line
            )
            .Where(predicate: item => !IsControlFlowStatement(statement: item.Statement))
            .Where(predicate: item =>
                (item.Previous != null && !HasBlankLineBetween(first: item.Previous, second: item.Statement))
                || (item.Next != null && !HasBlankLineBetween(first: item.Statement, second: item.Next))
            )
            .Select(selector: item =>
                CreateAnalysisItem(
                    code: "STXFORMAT008",
                    description: "Wrapped statements must be separated from adjacent method statements by an empty line.",
                    context: context,
                    location: item.Statement.GetLocation()
                )
            );

    private static IEnumerable<AnalysisItem> EvaluateSTXFORMAT009(EvaluationContext context) =>

        architectureModelQueries
            .GetDeclarations(context: context).SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.DescendantNodes())
            .OfType<MemberAccessExpressionSyntax>()
            .Where(
                predicate: (MemberAccessExpressionSyntax memberAccess) =>
                    memberAccess.Expression is InvocationExpressionSyntax
                    && memberAccess.Parent is InvocationExpressionSyntax
                    && memberAccess.Expression.GetLocation()
            .GetLineSpan().EndLinePosition.Line
                        == memberAccess.OperatorToken.GetLocation()
            .GetLineSpan().StartLinePosition.Line
            )
            .Select(
                selector: (MemberAccessExpressionSyntax memberAccess) =>
                    CreateAnalysisItem(
                        code: "STXFORMAT009",
                        description: "Every method chained from an invocation result must begin on a new line.",
                        context: context,
                        location: memberAccess.OperatorToken.GetLocation()
                    )
            );

    private static IEnumerable<AnalysisItem> EvaluateSTXFORMAT010(EvaluationContext context)
    {
        return (context.StandardElementType == StandardElementType.Test)
            ? Array.Empty<AnalysisItem>()
            : architectureModelQueries
                .GetDeclarations(context: context).SelectMany(
                    selector: (TypeDeclarationSyntax declaration) =>
                        declaration.DescendantTrivia(descendIntoChildren: null, descendIntoTrivia: false)
                )
                .Where(
                    predicate: (SyntaxTrivia trivia) =>
                        trivia.IsKind(kind: SyntaxKind.SingleLineCommentTrivia)
                        || trivia.IsKind(kind: SyntaxKind.MultiLineCommentTrivia)
                )
                .Where(predicate: (SyntaxTrivia trivia) => !IsDocumentationComment(trivia: trivia))
                .Where(predicate: (SyntaxTrivia trivia) => !IsCopyrightHeaderComment(trivia: trivia))
                .Select(
                    selector: (SyntaxTrivia trivia) =>
                        CreateAnalysisItem(
                            code: "STXFORMAT010",
                            description: "Production code must not contain non-documentation comments.",
                            context: context,
                            location: trivia.GetLocation()
                        )
                );
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXFORMAT011(EvaluationContext context) =>

        architectureModelQueries
            .GetDeclarations(context: context).GroupBy<TypeDeclarationSyntax, string>(
                keySelector: (TypeDeclarationSyntax declaration) => declaration.SyntaxTree.FilePath,
                comparer: StringComparer.OrdinalIgnoreCase
            )
            .Select(selector: (IGrouping<string, TypeDeclarationSyntax> declarations) => declarations.First())
            .Where(
                predicate: (TypeDeclarationSyntax declaration) =>
                    !HasCopyrightHeader(source: declaration.SyntaxTree.GetText()
            .ToString())
            )
            .Select(
                selector: (TypeDeclarationSyntax declaration) =>
                    CreateAnalysisItem(
                        code: "STXFORMAT011",
                        description: "Every C# source file must begin with a correctly formatted copyright header.",
                        context: context,
                        location: declaration
                            .SyntaxTree.GetRoot()
                            .GetFirstToken()
                            .GetLocation()
                    )
            );

    private static IEnumerable<AnalysisItem> EvaluateSTXFORMAT012(EvaluationContext context) =>

        architectureModelQueries
            .GetDeclarations(context: context).SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .Where(predicate: (MethodDeclarationSyntax method) => method.Modifiers.Any(kind: SyntaxKind.AsyncKeyword))
            .Where(
                predicate: (MethodDeclarationSyntax method) =>
                    method.ExpressionBody?.Expression is AwaitExpressionSyntax
            )
            .Select(
                selector: (MethodDeclarationSyntax method) =>
                    CreateAnalysisItem(
                        code: "STXFORMAT012",
                        description: "Expression-bodied methods must return awaitable calls directly instead of adding redundant async and await keywords.",
                        context: context,
                        location: method.GetLocation()
                    )
            );

    private static IEnumerable<AnalysisItem> EvaluateSTXFORMAT013(EvaluationContext context) =>

        architectureModelQueries
            .GetDeclarations(context: context).GroupBy(
                keySelector: (TypeDeclarationSyntax declaration) => declaration.SyntaxTree.FilePath,
                comparer: StringComparer.OrdinalIgnoreCase
            )
            .Select(
                selector: (IGrouping<string, TypeDeclarationSyntax> declarations) => declarations.First().SyntaxTree
            )
            .Select(
                selector: (SyntaxTree syntaxTree) =>
                    new
                    {
                        SyntaxTree = syntaxTree,
                        Position = FindInconsistentLineEnding(
                            source: syntaxTree.GetText()
            .ToString(),
                            projectLineEnding: architectureModelQueries.GetProjectLineEnding(context: context)
                        ),
                    }
            )
            .Where(predicate: item => item.Position >= 0)
            .Select(selector: item =>
                CreateAnalysisItem(
                    code: "STXFORMAT013",
                    description: "C# source files must use one consistent line-ending style throughout the project.",
                    context: context,
                    location: Location.Create(
                        syntaxTree: item.SyntaxTree,
                        textSpan: new TextSpan(start: item.Position, length: 1)
                    )
                )
            );

    private static int FindInconsistentLineEnding(string source, string projectLineEnding)
    {
        if (projectLineEnding.Length == 0)
        {
            return -1;
        }

        for (int index = 0; index < source.Length; index++)
        {
            if (source[index: index] == '\r')
            {
                string lineEnding = index < source.Length - 1 && source[index: index + 1] == '\n' ? "\r\n" : "\r";

                if (lineEnding != projectLineEnding)
                {
                    return index;
                }

                index += lineEnding.Length - 1;
            }
            else if (source[index: index] == '\n' && projectLineEnding != "\n")
            {
                return index;
            }
        }

        return -1;
    }

    private static InvocationExpressionSyntax? GetSingleInvocation(MethodDeclarationSyntax method)
    {
        BlockSyntax? body = method.Body;

        if (body == null || body.Statements.Count != 1)
        {
            return null;
        }

        StatementSyntax statementSyntax = body.Statements[index: 0];

        ExpressionSyntax? expressionSyntax = (
            (statementSyntax is ReturnStatementSyntax returnStatement)
                ? returnStatement.Expression
                : (
                    (!(statementSyntax is ExpressionStatementSyntax expressionStatement))
                        ? null
                        : expressionStatement.Expression
                )
        );

        ExpressionSyntax? expression = expressionSyntax;

        if (expression is AwaitExpressionSyntax awaitExpression)
        {
            expression = awaitExpression.Expression;
        }

        return expression as InvocationExpressionSyntax;
    }

    private static bool HasBlankLineBetween(SyntaxNode first, SyntaxNode second)
    {
        int firstEndLine = first.GetLocation()
            .GetLineSpan().EndLinePosition.Line;

        int secondStartLine = second.GetLocation()
            .GetLineSpan().StartLinePosition.Line;

        return secondStartLine - firstEndLine > 1;
    }

    private static bool HasCopyrightHeader(string source)
    {
        return TryGetCopyrightHeaderLength(source: source, headerLength: out _);
    }

    private static bool HasExactlyOneBlankLineBetween(SyntaxNode first, SyntaxNode second)
    {
        int firstEndLine = first.GetLocation()
            .GetLineSpan().EndLinePosition.Line;

        int secondStartLine = second.GetLocation()
            .GetLineSpan().StartLinePosition.Line;

        return secondStartLine - firstEndLine == 2;
    }

    private static bool HasTrailingNewlines(string source)
    {
        int trailingNewLines = 0;

        for (int index = source.Length - 1; index >= 0; index--)
        {
            if (source[index: index] == '\n')
            {
                trailingNewLines++;
                continue;
            }

            if (!char.IsWhiteSpace(c: source[index: index]))
            {
                break;
            }
        }

        return trailingNewLines > 0;
    }

    private static bool IsControlFlowStatement(StatementSyntax statement)
    {
        if (
            statement is IfStatementSyntax
            || statement is SwitchStatementSyntax
            || statement is ForStatementSyntax
            || statement is ForEachStatementSyntax
            || statement is WhileStatementSyntax
            || statement is DoStatementSyntax
        )
        {
            return true;
        }

        return false;
    }

    private static bool IsCopyrightBorder(string? line)
    {
        return line is not null
            && line.StartsWith(value: "// ", comparisonType: StringComparison.Ordinal)
            && line.Length > 3
            && line.Skip(count: 3)
            .All(predicate: character => character == '-');
    }

    private static bool IsCopyrightHeaderComment(SyntaxTrivia trivia)
    {
        SyntaxTree? syntaxTree = trivia.SyntaxTree;

        if (syntaxTree == null)
        {
            return false;
        }

        string source = syntaxTree.GetText()
            .ToString();

        return TryGetCopyrightHeaderLength(source: source, headerLength: out int headerLength)
            && trivia.SpanStart < headerLength;
    }

    private static bool IsDocumentationComment(SyntaxTrivia trivia)
    {
        string comment = trivia.ToFullString().TrimStart();

        return comment.StartsWith(value: "///", comparisonType: StringComparison.Ordinal)
            || comment.StartsWith(value: "/**", comparisonType: StringComparison.Ordinal);
    }

    private static string? ReadLine(string source, int position, out int nextPosition)
    {
        if (position >= source.Length)
        {
            nextPosition = source.Length + 1;
            return null;
        }

        int newlinePosition = source.IndexOf(value: '\n', startIndex: position);
        int lineEnd = newlinePosition < 0 ? source.Length : newlinePosition;

        if (lineEnd > position && source[index: lineEnd - 1] == '\r')
        {
            lineEnd--;
        }

        nextPosition = newlinePosition < 0 ? source.Length + 1 : newlinePosition + 1;
        return source.Substring(startIndex: position, length: lineEnd - position);
    }

    private static bool RequiresScopeBraces(StatementSyntax statement)
    {
        if (statement is BlockSyntax)
        {
            return false;
        }

        return statement.Parent switch
        {
            IfStatementSyntax ifStatement => ifStatement.Statement == statement,
            ElseClauseSyntax elseClause => elseClause.Statement == statement && statement is not IfStatementSyntax,
            ForStatementSyntax forStatement => forStatement.Statement == statement,
            ForEachStatementSyntax forEachStatement => forEachStatement.Statement == statement,
            ForEachVariableStatementSyntax forEachVariableStatement => forEachVariableStatement.Statement == statement,
            WhileStatementSyntax whileStatement => whileStatement.Statement == statement,
            DoStatementSyntax doStatement => doStatement.Statement == statement,
            UsingStatementSyntax usingStatement => usingStatement.Statement == statement,
            LockStatementSyntax lockStatement => lockStatement.Statement == statement,
            FixedStatementSyntax fixedStatement => fixedStatement.Statement == statement,
            _ => false,
        };
    }

    private static bool TryGetCopyrightHeaderLength(string source, out int headerLength)
    {
        headerLength = 0;
        string? openingBorder = ReadLine(source: source, position: 0, nextPosition: out int position);

        if (!IsCopyrightBorder(line: openingBorder))
        {
            return false;
        }

        int borderLength = openingBorder!.Length;
        bool containsCopyrightMessage = false;
        int messageCount = 0;

        while (position <= source.Length)
        {
            string? line = ReadLine(source: source, position: position, nextPosition: out int nextPosition);

            if (line == openingBorder)
            {
                headerLength = nextPosition;
                return messageCount > 0 && containsCopyrightMessage;
            }

            if (
                line is null
                || !line.StartsWith(value: "//", comparisonType: StringComparison.Ordinal)
                || line.Length >= borderLength
            )
            {
                return false;
            }

            containsCopyrightMessage |= line.Contains(
                value: "Copyright",
                comparisonType: StringComparison.OrdinalIgnoreCase
            );

            messageCount++;
            position = nextPosition;
        }

        return false;
    }
}
