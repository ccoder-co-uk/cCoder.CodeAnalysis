using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

if (args.Length != 1)
{
    throw new ArgumentException("Supply the project path to clean.");
}

MSBuildLocator.RegisterDefaults();

using MSBuildWorkspace workspace = MSBuildWorkspace.Create();
Project project = await workspace.OpenProjectAsync(Path.GetFullPath(args[0]));

foreach (Document document in project.Documents)
{
    string? filePath = document.FilePath;

    if (
        filePath is null
        || filePath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || filePath.Contains(
            $"{Path.DirectorySeparatorChar}RuleViolations{Path.DirectorySeparatorChar}",
            StringComparison.OrdinalIgnoreCase
        )
    )
    {
        continue;
    }

    SyntaxNode? root = await document.GetSyntaxRootAsync();
    SemanticModel? semanticModel = await document.GetSemanticModelAsync();

    if (root is null || semanticModel is null)
    {
        continue;
    }

    InvocationExpressionSyntax[] invocations = root
        .DescendantNodes()
        .OfType<InvocationExpressionSyntax>()
        .Where(invocation => invocation.Expression.ToString() != "nameof")
        .Where(invocation => invocation.ArgumentList.Arguments.Any(argument => argument.NameColon is null))
        .ToArray();

    SyntaxNode updatedRoot = root.ReplaceNodes(
        invocations,
        (original, rewritten) => AddArgumentNames(original, rewritten, semanticModel)
    );

    if (!filePath.EndsWith("InvalidIdentifierProcessingService.cs", StringComparison.OrdinalIgnoreCase))
    {
        MethodDeclarationSyntax[] reducibleMethods = updatedRoot
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(method => GetReducibleExpression(method) is not null)
            .ToArray();

        updatedRoot = updatedRoot.ReplaceNodes(
            reducibleMethods,
            (_, rewritten) => ReduceMethodBody(rewritten)
        );
    }

    string updatedSource = updatedRoot.ToFullString();

    if (!filePath.EndsWith("InvalidIdentifierProcessingService.cs", StringComparison.OrdinalIgnoreCase))
    {
        updatedSource = ApplyStatementSpacing(updatedSource);
        updatedSource = ApplyMethodChainLayout(updatedSource);
        updatedSource = ApplyExpressionBodyLayout(updatedSource);
        updatedSource = ApplyMethodSpacing(updatedSource);
    }

    if (!string.Equals(updatedSource, root.ToFullString(), StringComparison.Ordinal))
    {
        await File.WriteAllTextAsync(filePath, updatedSource);
        Console.WriteLine(filePath);
    }
}

static string ApplyExpressionBodyLayout(string source)
{
    SyntaxNode root = CSharpSyntaxTree.ParseText(source).GetRoot();
    MethodDeclarationSyntax[] methods = root
        .DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Where(method => method.ExpressionBody is not null)
        .Where(
            method =>
                method.ExpressionBody!.ArrowToken.GetLocation().GetLineSpan().StartLinePosition.Line
                == method.ExpressionBody.Expression.GetLocation().GetLineSpan().StartLinePosition.Line
        )
        .OrderByDescending(method => method.ExpressionBody!.ArrowToken.Span.End)
        .ToArray();

    foreach (MethodDeclarationSyntax method in methods)
    {
        int lineStart = source.LastIndexOf('\n', method.SpanStart) + 1;
        string indentation = new string(
            source.Skip(lineStart).TakeWhile(character => character is ' ' or '\t').ToArray()
        );
        source = source.Insert(
            method.ExpressionBody!.ArrowToken.Span.End,
            Environment.NewLine + indentation + "    "
        );
    }

    return source;
}

static string ApplyMethodSpacing(string source)
{
    SyntaxNode root = CSharpSyntaxTree.ParseText(source).GetRoot();
    HashSet<int> linesRequiringBlankBefore = new HashSet<int>();

    foreach (TypeDeclarationSyntax declaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
    {
        foreach ((MemberDeclarationSyntax first, MemberDeclarationSyntax second) in declaration.Members.Zip(
            declaration.Members.Skip(1)
        ))
        {
            if (first is MethodDeclarationSyntax && second is MethodDeclarationSyntax)
            {
                int firstEnd = first.GetLocation().GetLineSpan().EndLinePosition.Line;
                int secondStart = second.GetLocation().GetLineSpan().StartLinePosition.Line;

                if (secondStart - firstEnd < 2)
                {
                    linesRequiringBlankBefore.Add(secondStart);
                }
            }
        }
    }

    List<string> lines = source.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n').ToList();

    foreach (int line in linesRequiringBlankBefore.OrderByDescending(line => line))
    {
        lines.Insert(line, string.Empty);
    }

    return string.Join("\r\n", lines);
}

static string ApplyMethodChainLayout(string source)
{
    SyntaxNode root = CSharpSyntaxTree.ParseText(source).GetRoot();
    MemberAccessExpressionSyntax[] memberAccesses = root
        .DescendantNodes()
        .OfType<MemberAccessExpressionSyntax>()
        .Where(memberAccess => memberAccess.Expression is InvocationExpressionSyntax)
        .Where(memberAccess => memberAccess.Parent is InvocationExpressionSyntax)
        .Where(
            memberAccess =>
                memberAccess.Expression.GetLocation().GetLineSpan().EndLinePosition.Line
                == memberAccess.OperatorToken.GetLocation().GetLineSpan().StartLinePosition.Line
        )
        .OrderByDescending(memberAccess => memberAccess.OperatorToken.SpanStart)
        .ToArray();

    foreach (MemberAccessExpressionSyntax memberAccess in memberAccesses)
    {
        int lineStart = source.LastIndexOf('\n', memberAccess.OperatorToken.SpanStart - 1) + 1;
        string indentation = new string(
            source.Skip(lineStart).TakeWhile(character => character is ' ' or '\t').ToArray()
        );
        source = source.Insert(
            memberAccess.OperatorToken.SpanStart,
            Environment.NewLine + indentation + "    "
        );
    }

    return source;
}

static string ApplyStatementSpacing(string source)
{
    SyntaxNode root = CSharpSyntaxTree.ParseText(source).GetRoot();
    HashSet<int> linesRequiringBlankBefore = new HashSet<int>();

    foreach (BlockSyntax block in root.DescendantNodes().OfType<BlockSyntax>())
    {
        if (!block.Ancestors().OfType<MethodDeclarationSyntax>().Any())
        {
            continue;
        }

        for (int index = 0; index < block.Statements.Count; index++)
        {
            StatementSyntax statement = block.Statements[index];
            FileLinePositionSpan span = statement.GetLocation().GetLineSpan();
            bool isControlFlow = statement is IfStatementSyntax
                or SwitchStatementSyntax
                or ForStatementSyntax
                or ForEachStatementSyntax
                or WhileStatementSyntax
                or DoStatementSyntax;
            bool isWrappedStatement = span.StartLinePosition.Line != span.EndLinePosition.Line;

            if (!isControlFlow && !isWrappedStatement)
            {
                continue;
            }

            if (index > 0)
            {
                linesRequiringBlankBefore.Add(span.StartLinePosition.Line);
            }

            if (index < block.Statements.Count - 1)
            {
                linesRequiringBlankBefore.Add(
                    block.Statements[index + 1].GetLocation().GetLineSpan().StartLinePosition.Line
                );
            }
        }
    }

    List<string> lines = source.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n').ToList();

    foreach (int line in linesRequiringBlankBefore.OrderByDescending(line => line))
    {
        if (line > 0 && !string.IsNullOrWhiteSpace(lines[line - 1]))
        {
            lines.Insert(line, string.Empty);
        }
    }

    return string.Join("\r\n", lines);
}

static MethodDeclarationSyntax ReduceMethodBody(MethodDeclarationSyntax method)
{
    ExpressionSyntax expression = GetReducibleExpression(method)!;

    return method
        .WithBody(null)
        .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(expression))
        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
}

static ExpressionSyntax? GetReducibleExpression(MethodDeclarationSyntax method)
{
    if (method.Body?.Statements.Count != 1)
    {
        return null;
    }

    ExpressionSyntax? expression = method.Body.Statements[0] switch
    {
        ReturnStatementSyntax returnStatement => returnStatement.Expression,
        ExpressionStatementSyntax expressionStatement => expressionStatement.Expression,
        _ => null,
    };

    ExpressionSyntax candidate = expression is AwaitExpressionSyntax awaitExpression
        ? awaitExpression.Expression
        : expression!;

    if (candidate is not InvocationExpressionSyntax invocation)
    {
        return null;
    }

    FileLinePositionSpan lineSpan = invocation.GetLocation().GetLineSpan();

    return lineSpan.StartLinePosition.Line == lineSpan.EndLinePosition.Line ? null : expression;
}

static InvocationExpressionSyntax AddArgumentNames(
    InvocationExpressionSyntax original,
    InvocationExpressionSyntax rewritten,
    SemanticModel semanticModel)
{
    SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(original);
    IMethodSymbol? method = symbolInfo.Symbol as IMethodSymbol
        ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().SingleOrDefault();

    if (method is null)
    {
        return rewritten;
    }

    SeparatedSyntaxList<ArgumentSyntax> arguments = rewritten.ArgumentList.Arguments;
    int positionalIndex = 0;

    IParameterSymbol? paramsParameter = method.Parameters.LastOrDefault();

    if (paramsParameter?.IsParams == true && arguments.Count >= method.Parameters.Length)
    {
        int paramsIndex = method.Parameters.Length - 1;
        ArgumentSyntax[] paramsArguments = arguments.Skip(paramsIndex).ToArray();

        if (paramsArguments.All(argument => argument.NameColon is null && argument.RefKindKeyword.IsKind(SyntaxKind.None)))
        {
            CollectionExpressionSyntax collection = SyntaxFactory.CollectionExpression(
                SyntaxFactory.SeparatedList<CollectionElementSyntax>(
                    paramsArguments.Select(argument => SyntaxFactory.ExpressionElement(argument.Expression))
                )
            );
            ArgumentSyntax paramsArgument = SyntaxFactory
                .Argument(collection)
                .WithNameColon(
                    SyntaxFactory.NameColon(
                        SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(paramsParameter.Name))
                    )
                );
            arguments = SyntaxFactory.SeparatedList(arguments.Take(paramsIndex).Append(paramsArgument));
        }
    }

    for (int argumentIndex = 0; argumentIndex < arguments.Count; argumentIndex++)
    {
        ArgumentSyntax argument = arguments[argumentIndex];

        if (argument.NameColon is not null)
        {
            continue;
        }

        if (positionalIndex >= method.Parameters.Length)
        {
            return rewritten;
        }

        IParameterSymbol parameter = method.Parameters[positionalIndex++];
        NameColonSyntax nameColon = SyntaxFactory.NameColon(
            SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(parameter.Name))
        );
        arguments = arguments.Replace(argument, argument.WithNameColon(nameColon));
    }

    return rewritten.WithArgumentList(rewritten.ArgumentList.WithArguments(arguments));
}
