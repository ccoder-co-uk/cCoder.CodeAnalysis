// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class TestCodeAnalysisRulesProcessingService
    : CodeAnalysisRulesProcessingService,
        ITestCodeAnalysisRulesProcessingService
{
    public AnalysisItem[] Evaluate(EvaluationContext context)
    {
        List<AnalysisItem> list = new List<AnalysisItem>();
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateSourceFormatting(context));
        list.AddRange(EvaluateGenericTests(context));
        list.AddRange(EvaluateInheritedTestSuites(context));
        list.AddRange(EvaluateTestSuiteNames(context));
        list.AddRange(EvaluatePartialTestSuites(context));
        list.AddRange(EvaluateGivenWhenThenStructure(context));
        list.AddRange(EvaluateAcceptanceCrudCompleteness(context));
        return list.ToArray();
    }

    private static AnalysisItem[] EvaluateAcceptanceCrudCompleteness(EvaluationContext context)
    {
        if (
            !IsTestSuite(context)
            || !context.TypeName.EndsWith("ControllerAcceptanceTests", StringComparison.Ordinal)
            || context.TypeName.EndsWith("ImportControllerAcceptanceTests", StringComparison.Ordinal)
        )
        {
            return Array.Empty<AnalysisItem>();
        }
        string[] testMethodNames = (
            from method in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                .OfType<MethodDeclarationSyntax>()
            where
                method
                    .AttributeLists.SelectMany((AttributeListSyntax attributes) => attributes.Attributes)
                    .Any(
                        delegate(AttributeSyntax attribute)
                        {
                            string text = attribute.Name.ToString();
                            return (text == "Fact" || text == "FactAttribute") ? true : false;
                        }
                    )
            select method.Identifier.Text
        ).ToArray();
        string[] requiredOperations = new string[4] { "Get", "Post", "Put", "Delete" };
        return requiredOperations.All(
            (string requiredOperation) =>
                testMethodNames.Any(
                    (string testMethodName) => testMethodName.StartsWith(requiredOperation, StringComparison.Ordinal)
                )
        )
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXTEST006",
                    "An API acceptance suite must cover Get, Post, Put, and Delete operations.",
                    context,
                    context.Declarations[0].Identifier.GetLocation()
                ),
            };
    }

    private static AnalysisItem[] EvaluateGivenWhenThenStructure(EvaluationContext context)
    {
        return (
            from method in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                .OfType<MethodDeclarationSyntax>()
            where
                method
                    .AttributeLists.SelectMany((AttributeListSyntax attributes) => attributes.Attributes)
                    .Any(
                        delegate(AttributeSyntax attribute)
                        {
                            switch (attribute.Name.ToString())
                            {
                                case "Fact":
                                case "FactAttribute":
                                case "Theory":
                                case "TheoryAttribute":
                                    return true;
                                default:
                                    return false;
                            }
                        }
                    )
            where !HasGivenWhenThenComments(method)
            select CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                "STXTEST005",
                "Every test must explicitly separate its Given, When, and Then phases with comments.",
                context,
                method.Identifier.GetLocation()
            )
        ).ToArray();
    }

    private static bool HasGivenWhenThenComments(MethodDeclarationSyntax method)
    {
        string source = method.ToFullString();
        int given = source.IndexOf("// Given", StringComparison.Ordinal);
        int when = source.IndexOf("// When", StringComparison.Ordinal);
        int then = source.IndexOf("// Then", StringComparison.Ordinal);
        return given >= 0 && when > given && then > when;
    }

    private static AnalysisItem[] EvaluateGenericTests(EvaluationContext context)
    {
        return (
            from method in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                .OfType<MethodDeclarationSyntax>()
            where method.TypeParameterList != null
            select CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                "STXTEST001",
                "Tests and test helpers must not be generic.",
                context,
                method.GetLocation()
            )
        ).ToArray();
    }

    private static AnalysisItem[] EvaluateInheritedTestSuites(EvaluationContext context)
    {
        if (!IsTestSuite(context))
        {
            return Array.Empty<AnalysisItem>();
        }

        TypeDeclarationSyntax? declaration =
            context.Declarations.FirstOrDefault(
                (TypeDeclarationSyntax candidate) => candidate.BaseList != null);

        return !context.HasBaseClass
            ? Array.Empty<AnalysisItem>()
            :
            [
                CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXTEST002",
                    "Test suites must not inherit from base test classes.",
                    context,
                    declaration?.BaseList?.GetLocation()
                )
            ];
    }

    private static AnalysisItem[] EvaluateTestSuiteNames(EvaluationContext context)
    {
        if (!IsTestSuite(context))
        {
            return Array.Empty<AnalysisItem>();
        }

        return context.TypeName.EndsWith("Tests", StringComparison.Ordinal)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXTEST003",
                    "A test suite must be named for its target type using the {TargetType}Tests convention.",
                    context,
                    context.Declarations[0].Identifier.GetLocation()
                ),
            };
    }

    private static AnalysisItem[] EvaluatePartialTestSuites(EvaluationContext context)
    {
        if (!IsTestSuite(context))
        {
            return Array.Empty<AnalysisItem>();
        }

        return (
            !context.Declarations.Any(
                (TypeDeclarationSyntax declaration) =>
                    !declaration.Modifiers.Any((SyntaxToken modifier) => modifier.IsKind(SyntaxKind.PartialKeyword))
            )
        )
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXTEST004",
                    "Every declaration of a test suite must be partial.",
                    context,
                    context
                        .Declarations.First(
                            (TypeDeclarationSyntax declaration) =>
                                !declaration.Modifiers.Any(
                                    (SyntaxToken modifier) => modifier.IsKind(SyntaxKind.PartialKeyword)
                                )
                        )
                        .Identifier.GetLocation()
                ),
            };
    }

    private static bool IsTestSuite(EvaluationContext context) =>
        context.TypeName.EndsWith("Tests", StringComparison.Ordinal)
        || context.Declarations
            .SelectMany(
                selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .Any(predicate: method =>
                method.AttributeLists
                    .SelectMany(selector: attributes => attributes.Attributes)
                    .Any(predicate: attribute =>
                    {
                        string attributeName = attribute.Name.ToString();

                        return attributeName is "Fact"
                            or "FactAttribute"
                            or "Theory"
                            or "TheoryAttribute";
                    }));
}
