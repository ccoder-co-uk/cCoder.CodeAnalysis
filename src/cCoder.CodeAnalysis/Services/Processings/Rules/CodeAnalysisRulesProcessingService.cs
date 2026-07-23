// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal abstract class CodeAnalysisRulesProcessingService
{
    protected static AnalysisItem[] EvaluateSourceFormatting(EvaluationContext context)
    {
        List<AnalysisItem> list = new List<AnalysisItem>();
        list.AddRange(EvaluateCopyrightHeader(context));
        list.AddRange(EvaluateTrailingBlankLines(context));
        list.AddRange(EvaluateExpressionBodyLayout(context));
        list.AddRange(EvaluateControlFlowSpacing(context));
        list.AddRange(EvaluateMultilineStatementSpacing(context));
        list.AddRange(EvaluateMethodSpacing(context));
        list.AddRange(EvaluateNamedArguments(context));
        list.AddRange(EvaluateStandardFolderStructure(context));
        list.AddRange(EvaluateSingleCallMethodBodies(context));
        list.AddRange(EvaluateControlFlowBraces(context));
        list.AddRange(EvaluateMethodChainLayout(context));
        list.AddRange(EvaluateProductionComments(context));
        list.AddRange(EvaluateRedundantAsyncAwait(context));
        return list.ToArray();
    }

    private static AnalysisItem[] EvaluateRedundantAsyncAwait(EvaluationContext context)
    {
        return (
            from method in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                .OfType<MethodDeclarationSyntax>()
            where method.Modifiers.Any(SyntaxKind.AsyncKeyword)
            where method.ExpressionBody?.Expression is AwaitExpressionSyntax
            select CreateAnalysisItem(
                "STXFORMAT012",
                "Expression-bodied methods must return awaitable calls directly instead of adding redundant async and await keywords.",
                context,
                method.GetLocation()
            )
        ).ToArray();
    }

    private static AnalysisItem[] EvaluateProductionComments(EvaluationContext context)
    {
        return (context.StandardElementType == StandardElementType.Test)
            ? Array.Empty<AnalysisItem>()
            : (
                from trivia in context.Declarations.SelectMany(
                    (TypeDeclarationSyntax declaration) => declaration.DescendantTrivia(null, descendIntoTrivia: true)
                )
                where
                    trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                    || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                where !IsCopyrightHeaderComment(trivia)
                select CreateAnalysisItem(
                    "STXFORMAT010",
                    "Production code must not contain non-documentation comments.",
                    context,
                    trivia.GetLocation()
                )
            ).ToArray();
    }

    private static AnalysisItem[] EvaluateCopyrightHeader(EvaluationContext context)
    {
        return (
            from @group in context.Declarations.GroupBy<TypeDeclarationSyntax, string>(
                (TypeDeclarationSyntax declaration) => declaration.SyntaxTree.FilePath,
                StringComparer.OrdinalIgnoreCase
            )
            select @group.First() into declaration
            where !HasCopyrightHeader(declaration.SyntaxTree.GetText().ToString())
            select CreateAnalysisItem(
                "STXFORMAT011",
                "Every C# source file must begin with a correctly formatted copyright header.",
                context,
                declaration.SyntaxTree.GetRoot().GetFirstToken().GetLocation()
            )
        ).ToArray();
    }

    private static bool IsCopyrightHeaderComment(SyntaxTrivia trivia)
    {
        SyntaxTree? syntaxTree = trivia.SyntaxTree;
        if (syntaxTree == null)
        {
            return false;
        }
        string source = syntaxTree.GetText().ToString();
        return TryGetCopyrightHeaderLength(source, out int headerLength) && trivia.SpanStart < headerLength;
    }

    private static bool HasCopyrightHeader(string source)
    {
        return TryGetCopyrightHeaderLength(source, out _);
    }

    private static bool TryGetCopyrightHeaderLength(string source, out int headerLength)
    {
        headerLength = 0;
        string? openingBorder = ReadLine(source, 0, out int position);

        if (!IsCopyrightBorder(openingBorder))
        {
            return false;
        }

        int borderLength = openingBorder!.Length;

        bool containsCopyrightMessage = false;
        int messageCount = 0;

        while (position <= source.Length)
        {
            string? line = ReadLine(source, position, out int nextPosition);

            if (line == openingBorder)
            {
                headerLength = nextPosition;

                return messageCount > 0 && containsCopyrightMessage;
            }

            if (line is null || !line.StartsWith("//", StringComparison.Ordinal) || line.Length >= borderLength)
            {
                return false;
            }

            containsCopyrightMessage |= line.Contains("Copyright", StringComparison.OrdinalIgnoreCase);
            messageCount++;
            position = nextPosition;
        }

        return false;
    }

    private static bool IsCopyrightBorder(string? line)
    {
        return line is not null
            && line.StartsWith("// ", StringComparison.Ordinal)
            && line.Length > 3
            && line.Skip(3).All(character => character == '-');
    }

    private static string? ReadLine(string source, int position, out int nextPosition)
    {
        if (position >= source.Length)
        {
            nextPosition = source.Length + 1;

            return null;
        }

        int newlinePosition = source.IndexOf('\n', position);
        int lineEnd = newlinePosition < 0 ? source.Length : newlinePosition;

        if (lineEnd > position && source[lineEnd - 1] == '\r')
        {
            lineEnd--;
        }

        nextPosition = newlinePosition < 0 ? source.Length + 1 : newlinePosition + 1;

        return source.Substring(position, lineEnd - position);
    }

    private static AnalysisItem[] EvaluateMethodChainLayout(EvaluationContext context)
    {
        return (
            from memberAccess in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.DescendantNodes())
                .OfType<MemberAccessExpressionSyntax>()
            where memberAccess.Expression is InvocationExpressionSyntax
            where memberAccess.Parent is InvocationExpressionSyntax
            where
                memberAccess.Expression.GetLocation().GetLineSpan().EndLinePosition.Line
                == memberAccess.OperatorToken.GetLocation().GetLineSpan().StartLinePosition.Line
            select CreateAnalysisItem(
                "STXFORMAT009",
                "Every method chained from an invocation result must begin on a new line.",
                context,
                memberAccess.OperatorToken.GetLocation()
            )
        ).ToArray();
    }

    private static AnalysisItem[] EvaluateMultilineStatementSpacing(EvaluationContext context)
    {
        return (
            from item in (
                from block in context.Declarations.SelectMany(
                    (TypeDeclarationSyntax declaration) => declaration.DescendantNodes().OfType<BlockSyntax>()
                )
                where block.Ancestors().OfType<MethodDeclarationSyntax>().Any()
                select block
            ).SelectMany(
                (BlockSyntax block) =>
                    block.Statements.Select(
                        (StatementSyntax statement, int index) =>
                            new
                            {
                                Statement = statement,
                                Previous = ((index > 0) ? block.Statements[index - 1] : null),
                                Next = ((index < block.Statements.Count - 1) ? block.Statements[index + 1] : null),
                            }
                    )
            )
            where
                item.Statement.GetLocation().GetLineSpan().StartLinePosition.Line
                != item.Statement.GetLocation().GetLineSpan().EndLinePosition.Line
            where !IsControlFlowStatement(item.Statement)
            where
                (item.Previous != null && !HasBlankLineBetween(item.Previous, item.Statement))
                || (item.Next != null && !HasBlankLineBetween(item.Statement, item.Next))
            select CreateAnalysisItem(
                "STXFORMAT008",
                "Wrapped statements must be separated from adjacent method statements by an empty line.",
                context,
                item.Statement.GetLocation()
            )
        ).ToArray();
    }

    private static AnalysisItem[] EvaluateControlFlowBraces(EvaluationContext context)
    {
        return (
            from statement in context.Declarations.SelectMany(
                (TypeDeclarationSyntax declaration) => declaration.DescendantNodes().OfType<StatementSyntax>()
            )
            where RequiresScopeBraces(statement)
            select CreateAnalysisItem(
                "STXFORMAT007",
                "Control-flow statement bodies must always be enclosed in scope braces.",
                context,
                statement.GetLocation()
            )
        ).ToArray();
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
            ForEachVariableStatementSyntax forEachVariableStatement =>
                forEachVariableStatement.Statement == statement,
            WhileStatementSyntax whileStatement => whileStatement.Statement == statement,
            DoStatementSyntax doStatement => doStatement.Statement == statement,
            UsingStatementSyntax usingStatement => usingStatement.Statement == statement,
            LockStatementSyntax lockStatement => lockStatement.Statement == statement,
            FixedStatementSyntax fixedStatement => fixedStatement.Statement == statement,
            _ => false,
        };
    }

    private static AnalysisItem[] EvaluateSingleCallMethodBodies(EvaluationContext context)
    {
        return (
            from method in (
                from method in context
                    .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                    .OfType<MethodDeclarationSyntax>()
                where GetSingleInvocation(method) != null
                select method
            ).Where(
                delegate(MethodDeclarationSyntax method)
                {
                    InvocationExpressionSyntax singleInvocation = GetSingleInvocation(method)!;
                    FileLinePositionSpan lineSpan = singleInvocation.GetLocation().GetLineSpan();
                    return lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line;
                }
            )
            select CreateAnalysisItem(
                "STXFORMAT006",
                "A single multiline method call must be expressed as an expression-bodied method.",
                context,
                method.GetLocation()
            )
        ).ToArray();
    }

    private static InvocationExpressionSyntax? GetSingleInvocation(MethodDeclarationSyntax method)
    {
        BlockSyntax? body = method.Body;
        if (body == null || body.Statements.Count != 1)
        {
            return null;
        }
        StatementSyntax statementSyntax = body.Statements[0];
        if (1 == 0) { }
        ExpressionSyntax? expressionSyntax = (
            (statementSyntax is ReturnStatementSyntax returnStatement)
                ? returnStatement.Expression
                : (
                    (!(statementSyntax is ExpressionStatementSyntax expressionStatement))
                        ? null
                        : expressionStatement.Expression
                )
        );
        if (1 == 0) { }
        ExpressionSyntax? expression = expressionSyntax;
        if (expression is AwaitExpressionSyntax awaitExpression)
        {
            expression = awaitExpression.Expression;
        }
        return expression as InvocationExpressionSyntax;
    }

    private static AnalysisItem[] EvaluateStandardFolderStructure(EvaluationContext context)
    {
        return (
            from declaration in context.Declarations
            where !IsInStandardFolder(declaration.SyntaxTree.FilePath, context.StandardElementType)
            select CreateAnalysisItem(
                "STXSTRUCT001",
                "The source file must live in the standard folder for its element type.",
                context,
                declaration.GetLocation()
            )
        ).ToArray();
    }

    private static bool IsInStandardFolder(string filePath, StandardElementType elementType)
    {
        if (elementType == StandardElementType.Test)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return true;
        }
        string normalizedPath = filePath.Replace('\\', '/');
        if (1 == 0) { }
        string[] array = elementType switch
        {
            StandardElementType.Broker => new string[1] { "/Brokers/" },
            StandardElementType.Dependency => new string[3] { "/Brokers/", "/Dependencies/", "/Exposures/" },
            StandardElementType.Exposure => new string[2] { "/Exposures/", "/Controllers/" },
            StandardElementType.Model => new string[1] { "/Models/" },
            StandardElementType.FoundationService => new string[1] { "/Services/Foundations/" },
            StandardElementType.ProcessingService => new string[1] { "/Services/Processings/" },
            StandardElementType.OrchestrationService => new string[1] { "/Services/Orchestrations/" },
            StandardElementType.CoordinationService => new string[1] { "/Services/Coordinations/" },
            StandardElementType.ManagementService => new string[1] { "/Services/Managements/" },
            StandardElementType.AggregationService => new string[1] { "/Services/Aggregations/" },
            StandardElementType.Test => Array.Empty<string>(),
            _ => Array.Empty<string>(),
        };
        if (1 == 0) { }
        string[] expectedFolders = array;
        return expectedFolders.Length == 0
            || expectedFolders.Any(
                (string folder) => normalizedPath.Contains(folder, StringComparison.OrdinalIgnoreCase)
            );
    }

    private static AnalysisItem[] EvaluateTrailingBlankLines(EvaluationContext context)
    {
        return (
            from @group in context.Declarations.GroupBy<TypeDeclarationSyntax, string>(
                (TypeDeclarationSyntax declaration) => declaration.SyntaxTree.FilePath,
                StringComparer.OrdinalIgnoreCase
            )
            select @group.First() into declaration
            where HasTrailingNewlines(declaration.SyntaxTree.GetText().ToString())
            select CreateAnalysisItem(
                "STXFORMAT001",
                "Source files must not contain newline characters after the final code token.",
                context,
                declaration.SyntaxTree.GetRoot().GetLastToken().GetLocation()
            )
        ).ToArray();
    }

    private static AnalysisItem[] EvaluateExpressionBodyLayout(EvaluationContext context)
    {
        return (
            from method in (
                from method in context
                    .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                    .OfType<MethodDeclarationSyntax>()
                where method.ExpressionBody != null
                select method
            ).Where(
                delegate(MethodDeclarationSyntax method)
                {
                    LinePosition startLinePosition = method.GetLocation().GetLineSpan().StartLinePosition;
                    LinePosition startLinePosition2 = method
                        .ExpressionBody!.ArrowToken.GetLocation()
                        .GetLineSpan()
                        .StartLinePosition;
                    FileLinePositionSpan lineSpan = method.ExpressionBody.Expression.GetLocation().GetLineSpan();
                    int num = startLinePosition.Character + 4;
                    return startLinePosition2.Line == lineSpan.StartLinePosition.Line
                        || lineSpan.StartLinePosition.Character != num;
                }
            )
            select CreateAnalysisItem(
                "STXFORMAT002",
                "Expression-bodied method implementations must start on the line after the => token.",
                context,
                method.ExpressionBody!.ArrowToken.GetLocation()
            )
        ).ToArray();
    }

    private static AnalysisItem[] EvaluateControlFlowSpacing(EvaluationContext context)
    {
        return (
            from item in (
                from block in context.Declarations.SelectMany(
                    (TypeDeclarationSyntax declaration) => declaration.DescendantNodes().OfType<BlockSyntax>()
                )
                where block.Ancestors().OfType<MethodDeclarationSyntax>().Any()
                select block
            ).SelectMany(
                (BlockSyntax block) =>
                    block.Statements.Select(
                        (StatementSyntax statement, int index) =>
                            new
                            {
                                Statement = statement,
                                Previous = ((index > 0) ? block.Statements[index - 1] : null),
                                Next = ((index < block.Statements.Count - 1) ? block.Statements[index + 1] : null),
                            }
                    )
            )
            where IsControlFlowStatement(item.Statement)
            where
                (item.Previous != null && !HasBlankLineBetween(item.Previous, item.Statement))
                || (item.Next != null && !HasBlankLineBetween(item.Statement, item.Next))
            select CreateAnalysisItem(
                "STXFORMAT003",
                "Control-flow blocks must be separated from adjacent method statements by an empty line.",
                context,
                item.Statement.GetLocation()
            )
        ).ToArray();
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

    private static bool HasBlankLineBetween(SyntaxNode first, SyntaxNode second)
    {
        int firstEndLine = first.GetLocation().GetLineSpan().EndLinePosition.Line;
        int secondStartLine = second.GetLocation().GetLineSpan().StartLinePosition.Line;
        return secondStartLine - firstEndLine > 1;
    }

    private static AnalysisItem[] EvaluateMethodSpacing(EvaluationContext context)
    {
        return (
            from pair in context.Declarations.SelectMany(
                (TypeDeclarationSyntax declaration) =>
                    declaration.Members.Zip(
                        declaration.Members.Skip(1),
                        (MemberDeclarationSyntax first, MemberDeclarationSyntax second) => new
                        {
                            First = first,
                            Second = second,
                        }
                    )
            )
            where pair.First is MethodDeclarationSyntax && pair.Second is MethodDeclarationSyntax
            where !HasExactlyOneBlankLineBetween(pair.First, pair.Second)
            select CreateAnalysisItem(
                "STXFORMAT004",
                "Adjacent methods must have exactly one empty line between them.",
                context,
                pair.Second.GetLocation()
            )
        ).ToArray();
    }

    private static bool HasExactlyOneBlankLineBetween(SyntaxNode first, SyntaxNode second)
    {
        int firstEndLine = first.GetLocation().GetLineSpan().EndLinePosition.Line;
        int secondStartLine = second.GetLocation().GetLineSpan().StartLinePosition.Line;
        return secondStartLine - firstEndLine == 2;
    }

    private static AnalysisItem[] EvaluateNamedArguments(EvaluationContext context)
    {
        return (
            from invocation in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.DescendantNodes())
                .OfType<InvocationExpressionSyntax>()
            where invocation.Expression.ToString() != "nameof"
            where invocation.ArgumentList.Arguments.Any((ArgumentSyntax argument) => argument.NameColon == null)
            select CreateAnalysisItem(
                "STXFORMAT005",
                "Method call arguments must declare their parameter names.",
                context,
                invocation.GetLocation()
            )
        ).ToArray();
    }

    private static bool HasTrailingNewlines(string source)
    {
        int trailingNewLines = 0;

        for (int index = source.Length - 1; index >= 0; index--)
        {
            if (source[index] == '\n')
            {
                trailingNewLines++;
                continue;
            }

            if (!char.IsWhiteSpace(source[index]))
            {
                break;
            }
        }

        return trailingNewLines > 0;
    }

    protected static AnalysisItem[] EvaluatePropertiesAreNotAllowed(EvaluationContext context)
    {
        return (
            from property in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                .OfType<PropertyDeclarationSyntax>()
            select CreateAnalysisItem(
                "STX0002",
                "Properties may only be declared on models.",
                context,
                property.GetLocation()
            )
        ).ToArray();
    }

    protected static AnalysisItem[] EvaluateRedundantPassThroughService(EvaluationContext context)
    {
        MethodDeclarationSyntax[] methods = context
            .Declarations.Where(
                (TypeDeclarationSyntax declaration) =>
                    !declaration.SyntaxTree.FilePath.EndsWith(".Validations.cs", StringComparison.Ordinal)
                    && !declaration.SyntaxTree.FilePath.EndsWith(".Exceptions.cs", StringComparison.Ordinal)
            )
            .SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .ToArray();
        return (methods.Length == 0 || !methods.All(IsSinglePassThroughMethod))
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    "STX0003",
                    "A service containing only pass-through methods is redundant and should be retired.",
                    context
                ),
            };
    }

    protected static AnalysisItem[] EvaluateDependencyLayer(
        EvaluationContext context,
        StandardElementType expectedDependencyType,
        string code
    )
    {
        int count = context.Dependencies.Count;
        bool flag = (uint)(count - 2) <= 1u;
        bool hasValidCount = flag;
        bool containsOnlyExpectedDependencies = context.Dependencies.All(
            (TypeDependency dependency) => dependency.StandardElementType == expectedDependencyType
        );
        return (hasValidCount && containsOnlyExpectedDependencies)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code,
                    $"The service must have two or three {expectedDependencyType} dependencies.",
                    context
                ),
            };
    }

    protected static AnalysisItem[] EvaluateFlowForward(EvaluationContext context)
    {
        return (
            !context.Dependencies.Any(
                (TypeDependency dependency) => dependency.StandardElementType == context.StandardElementType
            )
        )
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    "STX0004",
                    "A service must not depend on another service at the same layer.",
                    context
                ),
            };
    }

    protected static AnalysisItem[] EvaluatePublicApiFlowForward(EvaluationContext context)
    {
        return context
            .PublicMethodCallLineNumbers.Select(
                (int lineNumber) =>
                    new AnalysisItem
                    {
                        Code = "STX0005",
                        Description =
                            "A public service method must not call another public method on the same service.",
                        Severity = AnalysisSeverity.Warning,
                        Type = context.TypeName,
                        LineNumber = lineNumber,
                    }
            )
            .ToArray();
    }

    protected static AnalysisItem[] EvaluateBusinessImplementationVisibility(EvaluationContext context)
    {
        return (!context.IsPublic)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    "STX0006",
                    "Business implementation classes should be internal by default.",
                    context
                ),
            };
    }

    protected static AnalysisItem[] EvaluateSingleServiceContract(EvaluationContext context)
    {
        return (context.PublicApiModelTypes.Count <= 1)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    "STX0007",
                    "A service public API must use one model contract or primitive types.",
                    context
                ),
            };
    }

    protected static AnalysisItem[] EvaluateServiceContractPattern(EvaluationContext context)
    {
        MethodDeclarationSyntax[] publicMethods = (
            from method in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                .OfType<MethodDeclarationSyntax>()
            where method.Modifiers.Any(SyntaxKind.PublicKeyword)
            select method
        ).ToArray();
        bool hasValidationPartial = context.Declarations.Any(
            (TypeDeclarationSyntax declaration) =>
                declaration.SyntaxTree.FilePath.EndsWith(".Validations.cs", StringComparison.Ordinal)
        );
        bool hasExceptionPartial = context.Declarations.Any(
            (TypeDeclarationSyntax declaration) =>
                declaration.SyntaxTree.FilePath.EndsWith(".Exceptions.cs", StringComparison.Ordinal)
        );
        bool allUseTryCatch = publicMethods.All(UsesTryCatch);
        bool allValidateInputs = publicMethods.All(ValidatesInputs);
        MethodDeclarationSyntax[] tryCatchMethods = (
            from method in context
                .Declarations.Where(
                    (TypeDeclarationSyntax declaration) =>
                        declaration.SyntaxTree.FilePath.EndsWith(".Exceptions.cs", StringComparison.Ordinal)
                )
                .SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                .OfType<MethodDeclarationSyntax>()
            where method.Identifier.Text == "TryCatch"
            select method
        ).ToArray();
        bool capturesValidationExceptions =
            tryCatchMethods.Length != 0
            && tryCatchMethods.All(
                (MethodDeclarationSyntax method) => HasWrappedExceptionCategory(method, "Validation")
            );
        bool capturesDependencyExceptions =
            tryCatchMethods.Length != 0
            && tryCatchMethods.All(
                (MethodDeclarationSyntax method) => HasWrappedExceptionCategory(method, "Dependency")
            );
        bool capturesDefaultExceptions = tryCatchMethods.Length != 0 && tryCatchMethods.All(HasWrappedDefaultException);
        bool usesRulesEngine = context
            .Declarations.Where(
                (TypeDeclarationSyntax declaration) =>
                    declaration.SyntaxTree.FilePath.EndsWith(".Validations.cs", StringComparison.Ordinal)
            )
            .SelectMany((TypeDeclarationSyntax declaration) => declaration.DescendantNodes())
            .OfType<InvocationExpressionSyntax>()
            .Any(
                (InvocationExpressionSyntax invocation) =>
                    invocation
                        .Expression.ToString()
                        .Contains("ValidationRulesEngine.Validate", StringComparison.Ordinal)
            );
        List<AnalysisItem> list = new List<AnalysisItem>();
        list.AddRange(
            CreateWhenInvalid(
                !hasValidationPartial,
                "STX0008",
                "A service must declare its validations in a Validations partial.",
                context
            )
        );
        list.AddRange(
            CreateWhenInvalid(
                !hasExceptionPartial,
                "STX0009",
                "A service must declare TryCatch handling in an Exceptions partial.",
                context
            )
        );
        list.AddRange(
            CreateWhenInvalid(
                !allUseTryCatch,
                "STX0010",
                "Every public service method must enter through a local TryCatch operation.",
                context
            )
        );
        list.AddRange(
            CreateWhenInvalid(
                !allValidateInputs,
                "STX0011",
                "Every service input must be validated inside TryCatch before business work.",
                context
            )
        );
        list.AddRange(
            CreateWhenInvalid(
                !usesRulesEngine,
                "STX0012",
                "Service validations must be evaluated through a rules engine.",
                context
            )
        );
        list.AddRange(
            CreateWhenInvalid(
                !capturesValidationExceptions,
                "STXEX001",
                "Every TryCatch overload must classify, wrap, and preserve validation exceptions.",
                context
            )
        );
        list.AddRange(
            CreateWhenInvalid(
                !capturesDependencyExceptions,
                "STXEX002",
                "Every TryCatch overload must classify, wrap, and preserve dependency exceptions.",
                context
            )
        );
        list.AddRange(
            CreateWhenInvalid(
                !capturesDefaultExceptions,
                "STXEX003",
                "Every TryCatch overload must wrap and preserve unclassified exceptions.",
                context
            )
        );
        list.AddRange(
            CreateWhenInvalid(
                context.ImplementedInterfaces.Count == 0,
                "STX0013",
                "A service must implement a local interface.",
                context
            )
        );
        list.AddRange(
            CreateWhenInvalid(
                !ImplementsMatchingInterface(context),
                "STX0014",
                "A service contract must be named after its implementation with an I prefix.",
                context
            )
        );
        list.AddRange(
            CreateWhenInvalid(
                !ContractContainsPublicMethods(context),
                "STX0015",
                "Every public service method must be declared by its local interface.",
                context
            )
        );
        list.AddRange(EvaluateDomainVocabulary(context));
        list.AddRange(EvaluateTypedIdentifiers(context));
        list.AddRange(EvaluateModelTypeNaming(context));
        list.AddRange(EvaluateCreationReturnTypeNaming(context));
        list.AddRange(EvaluateMutationNaming(context));
        return list.ToArray();
    }

    private static bool HasWrappedExceptionCategory(MethodDeclarationSyntax method, string categoryName)
    {
        return method
            .DescendantNodes()
            .OfType<CatchClauseSyntax>()
            .Any((CatchClauseSyntax catchClause) => CatchWrapsException(catchClause, categoryName));
    }

    private static bool HasWrappedDefaultException(MethodDeclarationSyntax method)
    {
        return method
            .DescendantNodes()
            .OfType<CatchClauseSyntax>()
            .Any(
                (CatchClauseSyntax catchClause) =>
                    catchClause.Declaration?.Type.ToString() == "Exception" && CatchWrapsException(catchClause, null)
            );
    }

    private static bool CatchWrapsException(CatchClauseSyntax catchClause, string? categoryName)
    {
        string caughtExceptionName = catchClause.Declaration?.Identifier.Text ?? string.Empty;
        return caughtExceptionName.Length > 0
            && (
                from throwStatement in catchClause.Block.DescendantNodes().OfType<ThrowStatementSyntax>()
                select throwStatement.Expression
            )
                .OfType<ObjectCreationExpressionSyntax>()
                .Any(
                    delegate(ObjectCreationExpressionSyntax objectCreation)
                    {
                        bool num;
                        if (categoryName != null)
                        {
                            num = objectCreation.Type.ToString().Contains(categoryName, StringComparison.Ordinal);
                        }
                        else
                        {
                            if (objectCreation.Type.ToString().Contains("Validation", StringComparison.Ordinal))
                            {
                                goto IL_0094;
                            }
                            num = !objectCreation.Type.ToString().Contains("Dependency", StringComparison.Ordinal);
                        }
                        if (!num)
                        {
                            goto IL_0094;
                        }
                        ArgumentListSyntax? argumentList = objectCreation.ArgumentList;
                        int result = (
                            (
                                argumentList != null
                                && argumentList.Arguments.Any(
                                    (ArgumentSyntax argument) =>
                                        argument
                                            .Expression.DescendantNodesAndSelf()
                                            .OfType<IdentifierNameSyntax>()
                                            .Any(
                                                (IdentifierNameSyntax identifier) =>
                                                    identifier.Identifier.Text == caughtExceptionName
                                            )
                                )
                            )
                                ? 1
                                : 0
                        );
                        goto IL_0095;
                        IL_0094:
                        result = 0;
                        goto IL_0095;
                        IL_0095:
                        return (byte)result != 0;
                    }
                );
    }

    private static AnalysisItem[] EvaluateDomainVocabulary(EvaluationContext context)
    {
        string[] nonDomainVerbs = new string[4] { "Select", "Insert", "Post", "Put" };
        return (
            from method in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                .OfType<MethodDeclarationSyntax>()
            where method.Modifiers.Any(SyntaxKind.PublicKeyword)
            where nonDomainVerbs.Any((string verb) => method.Identifier.Text.StartsWith(verb, StringComparison.Ordinal))
            select CreateAnalysisItem(
                "STX0016",
                "Service CRUD methods must use domain nouns: Get, Add, Update, or Delete.",
                context,
                method.GetLocation()
            )
        ).ToArray();
    }

    protected static AnalysisItem[] EvaluateTypedIdentifiers(EvaluationContext context)
    {
        return (
            from parameter in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                .OfType<MethodDeclarationSyntax>()
                .SelectMany((MethodDeclarationSyntax method) => method.ParameterList.Parameters)
            where parameter.Identifier.Text == "id"
            select CreateAnalysisItem(
                "STX0017",
                "Identifier parameters must be named for their type, for example studentId.",
                context,
                parameter.GetLocation()
            )
        ).ToArray();
    }

    protected static AnalysisItem[] EvaluateMutationNaming(EvaluationContext context)
    {
        if (context.TypeName.EndsWith(".IServiceCollectionExtensions", StringComparison.Ordinal))
        {
            return Array.Empty<AnalysisItem>();
        }

        List<AnalysisItem> items = new List<AnalysisItem>();
        foreach (
            MethodDeclarationSyntax method in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                .OfType<MethodDeclarationSyntax>()
        )
        {
            string methodName = method.Identifier.Text;
            string? operation = GetMutationOperation(methodName);
            if (operation == null)
            {
                continue;
            }
            (ParameterSyntax Parameter, string? TypeName) modelParameter = method
                .ParameterList.Parameters.Select(
                    (ParameterSyntax parameter) =>
                        (Parameter: parameter, TypeName: GetModelTypeName(parameter, context))
                )
                .FirstOrDefault(item => item.TypeName is not null);
            string? modelTypeName = modelParameter.TypeName;
            if (modelTypeName == null)
            {
                continue;
            }
            string? expectedPrefix = operation switch
            {
                "create" when !methodName.StartsWith("AddOrUpdate", StringComparison.Ordinal) => "new",
                "update" => "updated",
                "delete" => "deleted",
                _ => null,
            };
            if (
                expectedPrefix != null
                && !modelParameter.Item1.Identifier.Text.StartsWith(expectedPrefix, StringComparison.Ordinal)
            )
            {
                string code = operation switch
                {
                    "create" => "STX0019",
                    "update" => "STX0020",
                    _ => "STX0021",
                };
                items.Add(
                    CreateAnalysisItem(
                        code,
                        operation + " model parameters must use the " + expectedPrefix + " prefix.",
                        context,
                        modelParameter.Item1.GetLocation()
                    )
                );
            }
        }
        return items.ToArray();
    }

    private static AnalysisItem[] EvaluateModelTypeNaming(EvaluationContext context)
    {
        return (
            from method in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                .OfType<MethodDeclarationSyntax>()
            where method.Modifiers.Any(SyntaxKind.PublicKeyword)
            select new
            {
                Method = method,
                ModelTypes = (
                    from parameter in method.ParameterList.Parameters
                    select GetModelTypeName(parameter, context) into typeName
                    where typeName != null
                    select typeName
                )
                    .Distinct<string>(StringComparer.Ordinal)
                    .ToArray(),
            } into item
            where
                item.ModelTypes.Any(
                    (string typeName) => !item.Method.Identifier.Text.Contains(typeName, StringComparison.Ordinal)
                )
            select CreateAnalysisItem(
                "STX0018",
                "Service method names must include each model type they operate on.",
                context,
                item.Method.GetLocation()
            )
        ).ToArray();
    }

    protected static AnalysisItem[] EvaluateCreationReturnTypeNaming(EvaluationContext context)
    {
        return (
            from method in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                .OfType<MethodDeclarationSyntax>()
            where method.Identifier.Text.StartsWith("Create", StringComparison.Ordinal)
            select new
            {
                Method = method,
                ReturnType = method
                    .ReturnType.DescendantNodesAndSelf()
                    .OfType<SimpleNameSyntax>()
                    .LastOrDefault()
                    ?.Identifier.Text,
            } into item
            where item.ReturnType != null
            where !item.Method.Identifier.Text.Contains(item.ReturnType, StringComparison.Ordinal)
            select CreateAnalysisItem(
                "STX0022",
                "Creation method names must include the concrete type they create.",
                context,
                item.Method.GetLocation()
            )
        ).ToArray();
    }

    private static string? GetMutationOperation(string methodName)
    {
        if (
            methodName.StartsWith("Add", StringComparison.Ordinal)
            || methodName.StartsWith("Insert", StringComparison.Ordinal)
            || methodName.StartsWith("Post", StringComparison.Ordinal)
        )
        {
            return "create";
        }
        if (
            methodName.StartsWith("Update", StringComparison.Ordinal)
            || methodName.StartsWith("Put", StringComparison.Ordinal)
        )
        {
            return "update";
        }
        return methodName.StartsWith("Delete", StringComparison.Ordinal) ? "delete" : null;
    }

    private static string? GetModelTypeName(ParameterSyntax parameter, EvaluationContext context)
    {
        string[] candidateNames =
            (
                from name in parameter.Type?.DescendantNodesAndSelf().OfType<SimpleNameSyntax>()
                select name.Identifier.Text
            )
                .Reverse()
                .ToArray()
            ?? Array.Empty<string>();
        return candidateNames.FirstOrDefault(
            (string candidate) =>
                context.PublicApiModelTypes.Any(
                    (string modelType) => modelType.EndsWith("." + candidate, StringComparison.Ordinal)
                )
        );
    }

    private static bool ImplementsMatchingInterface(EvaluationContext context)
    {
        if (context.ImplementedInterfaces.Count == 0)
        {
            return true;
        }
        string typeName = context.TypeName.Split('.').Last();
        string expectedInterfaceName = "I" + typeName;
        return context.ImplementedInterfaces.Any(
            (string interfaceName) => interfaceName.Split('.').Last() == expectedInterfaceName
        );
    }

    private static bool ContractContainsPublicMethods(EvaluationContext context)
    {
        return context.ImplementedInterfaces.Count == 0
            || context.PublicMethodNames.All(((IEnumerable<string>)context.ContractMethodNames).Contains<string>);
    }

    private static AnalysisItem[] CreateWhenInvalid(
        bool isInvalid,
        string code,
        string description,
        EvaluationContext context
    )
    {
        return (!isInvalid)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1] { CreateAnalysisItem(code, description, context) };
    }

    private static bool UsesTryCatch(MethodDeclarationSyntax method)
    {
        return method
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Any(
                (InvocationExpressionSyntax invocation) =>
                    IsTryCatchInvocation(invocation.Expression)
                    && invocation.ArgumentList.Arguments.Any(
                        (ArgumentSyntax argument) => argument.Expression is LambdaExpressionSyntax
                    )
            );
    }

    private static bool IsTryCatchInvocation(ExpressionSyntax expression)
    {
        return expression is IdentifierNameSyntax identifierName && identifierName.Identifier.Text == "TryCatch"
            || expression is GenericNameSyntax genericName && genericName.Identifier.Text == "TryCatch";
    }

    private static bool ValidatesInputs(MethodDeclarationSyntax method)
    {
        string[] parameters = (
            from parameter in method.ParameterList.Parameters.Where(
                delegate(ParameterSyntax parameter)
                {
                    EqualsValueClauseSyntax? equalsValueClauseSyntax = parameter.Default;
                    return equalsValueClauseSyntax == null
                        || !equalsValueClauseSyntax.Value.IsKind(SyntaxKind.NullLiteralExpression);
                }
            )
            select parameter.Identifier.Text
        ).ToArray();
        if (parameters.Length == 0)
        {
            return true;
        }
        InvocationExpressionSyntax? validation = method
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault((InvocationExpressionSyntax invocation) => invocation.Expression.ToString() == "Validate");
        return validation != null
            && parameters.All(
                (string parameter) =>
                    validation.ArgumentList.Arguments.Any(
                        (ArgumentSyntax argument) =>
                            argument
                                .Expression.DescendantNodesAndSelf()
                                .OfType<IdentifierNameSyntax>()
                                .Any((IdentifierNameSyntax identifier) => identifier.Identifier.Text == parameter)
                    )
            );
    }

    protected static AnalysisItem CreateAnalysisItem(
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
            LineNumber = (
                location is not null ? location.GetLineSpan().StartLinePosition.Line + 1 : context.LineNumber
            ),
        };
    }

    private static bool IsSinglePassThroughMethod(MethodDeclarationSyntax method)
    {
        bool flag2;
        bool flag3;
        if (
            method
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault(
                    (InvocationExpressionSyntax invocation) => invocation.Expression.ToString() == "TryCatch"
                )
                ?.ArgumentList.Arguments.FirstOrDefault()
                ?.Expression
            is LambdaExpressionSyntax lambda
        )
        {
            CSharpSyntaxNode body = lambda.Body;
            if (1 == 0) { }
            SyntaxList<StatementSyntax> syntaxList = (
                (body is BlockSyntax block)
                    ? block.Statements
                    : (
                        (!(body is ExpressionSyntax expression))
                            ? SyntaxList.Create(default(ReadOnlySpan<StatementSyntax>))
                            : SyntaxList.Create(
                                new ReadOnlySpan<StatementSyntax>(
                                    new[] { (StatementSyntax)SyntaxFactory.ReturnStatement(expression) }
                                )
                            )
                    )
            );
            if (1 == 0) { }
            IEnumerable<StatementSyntax> statements = syntaxList;
            StatementSyntax[] businessStatements = statements
                .Where(
                    (StatementSyntax statement) =>
                        !statement.ToString().StartsWith("Validate(", StringComparison.Ordinal)
                )
                .ToArray();
            bool flag = businessStatements.Length == 1;
            flag2 = flag;
            if (flag2)
            {
                StatementSyntax statementSyntax = businessStatements.Single();
                if (1 == 0) { }
                if (!(statementSyntax is ReturnStatementSyntax item))
                {
                    if (!(statementSyntax is ExpressionStatementSyntax item2))
                    {
                        goto IL_016e;
                    }
                    flag3 = IsInvocation(item2.Expression);
                }
                else
                {
                    if (item.Expression == null)
                    {
                        goto IL_016e;
                    }
                    flag3 = IsInvocation(item.Expression);
                }
                goto IL_0173;
            }
            goto IL_017b;
        }
        if (method.ExpressionBody != null)
        {
            return IsInvocation(method.ExpressionBody.Expression);
        }
        BlockSyntax? body2 = method.Body;
        if (body2 == null || body2.Statements.Count != 1)
        {
            return false;
        }
        StatementSyntax statementSyntax2 = method.Body!.Statements.Single();
        if (1 == 0) { }
        if (!(statementSyntax2 is ReturnStatementSyntax returnStatement))
        {
            if (!(statementSyntax2 is ExpressionStatementSyntax expressionStatement))
            {
                goto IL_023f;
            }
            flag3 = IsInvocation(expressionStatement.Expression);
        }
        else
        {
            if (returnStatement.Expression == null)
            {
                goto IL_023f;
            }
            flag3 = IsInvocation(returnStatement.Expression);
        }
        goto IL_0244;
        IL_017b:
        return flag2;
        IL_0173:
        if (1 == 0) { }
        flag2 = flag3;
        goto IL_017b;
        IL_0244:
        if (1 == 0) { }
        return flag3;
        IL_023f:
        flag3 = false;
        goto IL_0244;
        IL_016e:
        flag3 = false;
        goto IL_0173;
    }

    private static bool IsInvocation(ExpressionSyntax expression)
    {
        if (expression is AwaitExpressionSyntax awaitExpression)
        {
            return IsInvocation(awaitExpression.Expression);
        }
        return expression is InvocationExpressionSyntax invocation
            && invocation.ArgumentList.Arguments.All(
                (ArgumentSyntax argument) => argument.Expression is IdentifierNameSyntax
            );
    }
}