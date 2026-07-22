using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

string repositoryPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
string sourcePath = Path.Combine(repositoryPath, "src");

string[] referencePaths = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
    .Split(Path.PathSeparator)
    .Concat(
        Directory
            .GetFiles(sourcePath, "*.dll", SearchOption.AllDirectories)
            .Where(path =>
                path.Contains(
                    $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase
                )
            )
    )
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

MetadataReference[] references = referencePaths.Select(path => MetadataReference.CreateFromFile(path)).ToArray();

foreach (string projectPath in Directory.GetFiles(sourcePath, "*.csproj", SearchOption.AllDirectories))
{
    string projectDirectory = Path.GetDirectoryName(projectPath)!;
    string[] filePaths = Directory
        .GetFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
        .Where(path =>
            !path.Contains(
                $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                StringComparison.OrdinalIgnoreCase
            )
        )
        .Where(path =>
            !path.Contains(
                $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                StringComparison.OrdinalIgnoreCase
            )
        )
        .ToArray();

    SyntaxTree[] syntaxTrees = filePaths
        .Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), path: path))
        .ToArray();

    SyntaxTree implicitUsings = CSharpSyntaxTree.ParseText(
        "global using System;\n"
            + "global using System.Collections.Generic;\n"
            + "global using System.IO;\n"
            + "global using System.Linq;\n"
            + "global using System.Threading;\n"
            + "global using System.Threading.Tasks;\n"
    );

    CSharpCompilation compilation = CSharpCompilation.Create(
        Path.GetFileNameWithoutExtension(projectPath),
        syntaxTrees.Append(implicitUsings),
        references
    );

    foreach (SyntaxTree syntaxTree in syntaxTrees)
    {
        SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
        var rewriter = new ArgumentNameRewriter(semanticModel);
        SyntaxNode rewrittenRoot = rewriter.Visit(syntaxTree.GetRoot());
        var singleCallRewriter = new SingleCallMethodRewriter();
        rewrittenRoot = singleCallRewriter.Visit(rewrittenRoot);
        var scopeBraceRewriter = new ScopeBraceRewriter(syntaxTree.FilePath);
        rewrittenRoot = scopeBraceRewriter.Visit(rewrittenRoot);
        var multilineSpacingRewriter = new MultilineStatementSpacingRewriter(syntaxTree.FilePath);
        rewrittenRoot = multilineSpacingRewriter.Visit(rewrittenRoot);

        if (
            !rewriter.Changed
            && !singleCallRewriter.Changed
            && !scopeBraceRewriter.Changed
            && !multilineSpacingRewriter.Changed
        )
        {
            continue;
        }

        File.WriteAllText(syntaxTree.FilePath, rewrittenRoot.ToFullString(), new System.Text.UTF8Encoding(false));
    }
}

internal sealed class MultilineStatementSpacingRewriter(string filePath) : CSharpSyntaxRewriter
{
    public bool Changed { get; private set; }

    public override SyntaxNode? VisitBlock(BlockSyntax node)
    {
        BlockSyntax visited = (BlockSyntax)base.VisitBlock(node)!;

        if (
            filePath.EndsWith("InvalidIdentifierProcessingService.cs", StringComparison.OrdinalIgnoreCase)
            || visited.Statements.Count < 2
        )
        {
            return visited;
        }

        SyntaxList<StatementSyntax> statements = visited.Statements;

        for (int index = 1; index < statements.Count; index++)
        {
            StatementSyntax previous = statements[index - 1];
            StatementSyntax current = statements[index];

            if (!IsWrapped(statement: previous) && !IsWrapped(statement: current))
            {
                continue;
            }

            int lineBreaks =
                previous.GetTrailingTrivia().Count(predicate: trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                + current.GetLeadingTrivia().Count(predicate: trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));

            if (lineBreaks >= 2)
            {
                continue;
            }

            statements = statements.Replace(
                current,
                current.WithLeadingTrivia(
                    current.GetLeadingTrivia().Insert(index: 0, SyntaxFactory.ElasticCarriageReturnLineFeed)
                )
            );
            this.Changed = true;
        }

        return visited.WithStatements(statements);
    }

    private static bool IsWrapped(StatementSyntax statement) =>
        statement
            is not IfStatementSyntax
                and not SwitchStatementSyntax
                and not ForStatementSyntax
                and not ForEachStatementSyntax
                and not ForEachVariableStatementSyntax
                and not WhileStatementSyntax
                and not DoStatementSyntax
        && statement.WithoutTrivia().ToFullString().Contains(value: '\n', comparisonType: StringComparison.Ordinal);
}

internal sealed class ScopeBraceRewriter(string filePath) : CSharpSyntaxRewriter
{
    public bool Changed { get; private set; }

    public override SyntaxNode? VisitIfStatement(IfStatementSyntax node) =>
        base.VisitIfStatement(node.WithStatement(AddBraces(statement: node.Statement)));

    public override SyntaxNode? VisitElseClause(ElseClauseSyntax node) =>
        base.VisitElseClause(
            node.Statement is IfStatementSyntax ? node : node.WithStatement(AddBraces(statement: node.Statement))
        );

    public override SyntaxNode? VisitForStatement(ForStatementSyntax node) =>
        base.VisitForStatement(node.WithStatement(AddBraces(statement: node.Statement)));

    public override SyntaxNode? VisitForEachStatement(ForEachStatementSyntax node) =>
        base.VisitForEachStatement(node.WithStatement(AddBraces(statement: node.Statement)));

    public override SyntaxNode? VisitForEachVariableStatement(ForEachVariableStatementSyntax node) =>
        base.VisitForEachVariableStatement(node.WithStatement(AddBraces(statement: node.Statement)));

    public override SyntaxNode? VisitWhileStatement(WhileStatementSyntax node) =>
        base.VisitWhileStatement(node.WithStatement(AddBraces(statement: node.Statement)));

    public override SyntaxNode? VisitDoStatement(DoStatementSyntax node) =>
        base.VisitDoStatement(node.WithStatement(AddBraces(statement: node.Statement)));

    public override SyntaxNode? VisitUsingStatement(UsingStatementSyntax node) =>
        base.VisitUsingStatement(node.WithStatement(AddBraces(statement: node.Statement)));

    public override SyntaxNode? VisitLockStatement(LockStatementSyntax node) =>
        base.VisitLockStatement(node.WithStatement(AddBraces(statement: node.Statement)));

    public override SyntaxNode? VisitFixedStatement(FixedStatementSyntax node) =>
        base.VisitFixedStatement(node.WithStatement(AddBraces(statement: node.Statement)));

    private StatementSyntax AddBraces(StatementSyntax statement)
    {
        if (
            statement is BlockSyntax
            || filePath.EndsWith("InvalidIdentifierProcessingService.cs", StringComparison.OrdinalIgnoreCase)
        )
        {
            return statement;
        }

        this.Changed = true;

        return SyntaxFactory.Block(statement).WithTriviaFrom(statement);
    }
}

internal sealed class SingleCallMethodRewriter : CSharpSyntaxRewriter
{
    public bool Changed { get; private set; }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        MethodDeclarationSyntax visited = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node)!;

        if (
            visited.SyntaxTree.FilePath.EndsWith(
                "InvalidIdentifierProcessingService.cs",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return visited;
        }

        if (visited.Body?.Statements.Count != 1)
        {
            return visited;
        }

        ExpressionSyntax? expression = visited.Body.Statements[0] switch
        {
            ReturnStatementSyntax returnStatement => returnStatement.Expression,
            ExpressionStatementSyntax expressionStatement => expressionStatement.Expression,
            _ => null,
        };

        ExpressionSyntax invocationCandidate = expression is AwaitExpressionSyntax awaitExpression
            ? awaitExpression.Expression
            : expression!;

        if (
            invocationCandidate is not InvocationExpressionSyntax invocation
            || invocation.GetLocation().GetLineSpan().StartLinePosition.Line
                == invocation.GetLocation().GetLineSpan().EndLinePosition.Line
        )
        {
            return visited;
        }

        this.Changed = true;

        return visited
            .WithBody(null)
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(expression!))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }
}

internal sealed class ArgumentNameRewriter(SemanticModel semanticModel) : CSharpSyntaxRewriter
{
    public bool Changed { get; private set; }

    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (
            node.SyntaxTree.FilePath.EndsWith(
                "InvalidIdentifierProcessingService.cs",
                StringComparison.OrdinalIgnoreCase
            )
            && node.Expression.ToString() == "Format"
        )
        {
            return base.VisitInvocationExpression(node);
        }

        InvocationExpressionSyntax visited = (InvocationExpressionSyntax)base.VisitInvocationExpression(node)!;

        if (semanticModel.GetSymbolInfo(node).Symbol is not IMethodSymbol method)
        {
            return visited;
        }

        SeparatedSyntaxList<ArgumentSyntax> arguments = visited.ArgumentList.Arguments;
        IParameterSymbol? paramsParameter = method.Parameters.LastOrDefault(parameter => parameter.IsParams);

        if (paramsParameter is not null)
        {
            int paramsIndex = method.Parameters.IndexOf(paramsParameter);

            if (arguments.Count > paramsIndex + 1)
            {
                string expressions = string.Join(
                    ", ",
                    arguments.Skip(paramsIndex).Select(argument => argument.Expression.ToString())
                );
                ArgumentSyntax paramsArgument = SyntaxFactory
                    .Argument(SyntaxFactory.ParseExpression($"[{expressions}]"))
                    .WithNameColon(SyntaxFactory.NameColon(paramsParameter.Name));

                arguments = SyntaxFactory.SeparatedList(arguments.Take(paramsIndex).Append(paramsArgument));
                this.Changed = true;
            }
        }

        for (int index = 0; index < arguments.Count; index++)
        {
            ArgumentSyntax argument = arguments[index];
            SyntaxTriviaList argumentTrivia = argument.GetLeadingTrivia();
            int invocationLine = node.GetLocation().GetLineSpan().StartLinePosition.Line;
            int argumentLine = node
                .ArgumentList.Arguments[Math.Min(index, node.ArgumentList.Arguments.Count - 1)]
                .GetLocation()
                .GetLineSpan()
                .StartLinePosition.Line;
            int argumentIndent = node.GetLocation().GetLineSpan().StartLinePosition.Character + 4;

            if (argument.NameColon is not null && argumentLine > invocationLine)
            {
                argument = argument
                    .WithoutLeadingTrivia()
                    .WithNameColon(
                        argument.NameColon.WithName(
                            argument
                                .NameColon.Name.WithoutLeadingTrivia()
                                .WithLeadingTrivia(SyntaxFactory.Whitespace(new string(' ', argumentIndent)))
                        )
                    );
                arguments = arguments.Replace(arguments[index], argument);
                this.Changed = true;
            }

            if (argument.NameColon is not null)
            {
                continue;
            }

            IParameterSymbol? parameter = GetParameter(method, index);

            if (parameter is null)
            {
                continue;
            }

            arguments = arguments.Replace(
                argument,
                argument
                    .WithoutLeadingTrivia()
                    .WithNameColon(
                        SyntaxFactory.NameColon(
                            SyntaxFactory
                                .IdentifierName(parameter.Name)
                                .WithoutLeadingTrivia()
                                .WithLeadingTrivia(
                                    argumentLine > invocationLine
                                        ? SyntaxFactory.TriviaList(
                                            SyntaxFactory.Whitespace(new string(' ', argumentIndent))
                                        )
                                        : argumentTrivia
                                )
                        )
                    )
            );
            this.Changed = true;
        }

        return visited.WithArgumentList(visited.ArgumentList.WithArguments(arguments));
    }

    private static IParameterSymbol? GetParameter(IMethodSymbol method, int argumentIndex)
    {
        if (argumentIndex < method.Parameters.Length)
        {
            return method.Parameters[argumentIndex];
        }

        return method.Parameters.LastOrDefault(parameter => parameter.IsParams);
    }
}