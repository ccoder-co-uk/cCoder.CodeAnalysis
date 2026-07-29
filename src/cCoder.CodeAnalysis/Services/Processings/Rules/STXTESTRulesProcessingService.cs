// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXTESTRulesProcessingService : ISTXTESTRulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        foreach (AnalysisItem item in EvaluateSTXTEST001(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXTEST002(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXTEST003(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXTEST004(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXTEST005(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXTEST006(context: context))
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

    private static IEnumerable<AnalysisItem> EvaluateSTXTEST006(EvaluationContext context)
    {
        if (
            !IsTestSuite(context: context)
            || !context.TypeName.EndsWith(value: "ControllerAcceptanceTests", comparisonType: StringComparison.Ordinal)
            || context.TypeName.EndsWith(
                value: "ImportControllerAcceptanceTests",
                comparisonType: StringComparison.Ordinal
            )
        )
        {
            return Array.Empty<AnalysisItem>();
        }

        string[] testMethodNames = context
            .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .Where(
                predicate: (MethodDeclarationSyntax method) =>
                    method
                        .AttributeLists.SelectMany(selector: (AttributeListSyntax attributes) => attributes.Attributes)
            .Any(
                            predicate: delegate (AttributeSyntax attribute)
                            {
                                string text = attribute.Name.ToString();
                                return (text == "Fact" || text == "FactAttribute") ? true : false;
                            }
                        )
            )
            .Select(selector: (MethodDeclarationSyntax method) => method.Identifier.Text)
            .ToArray();

        string[] requiredOperations = new string[4] { "Get", "Post", "Put", "Delete" };

        return requiredOperations.All(
            predicate: (string requiredOperation) =>
                testMethodNames.Any(
                    predicate: (string testMethodName) =>
                        testMethodName.StartsWith(value: requiredOperation, comparisonType: StringComparison.Ordinal)
                )
        )
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: "STXTEST006",
                    description: "An API acceptance suite must cover Get, Post, Put, and Delete operations.",
                    context: context,
                    location: context.Declarations[index: 0].Identifier.GetLocation()
                ),
            };
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXTEST005(EvaluationContext context) =>

        context
            .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .Where(
                predicate: (MethodDeclarationSyntax method) =>
                    method
                        .AttributeLists.SelectMany(selector: (AttributeListSyntax attributes) => attributes.Attributes)
            .Any(
                            predicate: delegate (AttributeSyntax attribute)
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
            )
            .Where(predicate: (MethodDeclarationSyntax method) => !HasGivenWhenThenComments(method: method))
            .Select(
                selector: (MethodDeclarationSyntax method) =>
                    CreateAnalysisItem(
                        code: "STXTEST005",
                        description: "Every test must explicitly separate its Given, When, and Then phases with comments.",
                        context: context,
                        location: method.Identifier.GetLocation()
                    )
            );

    private static bool HasGivenWhenThenComments(MethodDeclarationSyntax method)
    {
        string source = method.ToFullString();
        int given = source.IndexOf(value: "// Given", comparisonType: StringComparison.Ordinal);
        int when = source.IndexOf(value: "// When", comparisonType: StringComparison.Ordinal);
        int then = source.IndexOf(value: "// Then", comparisonType: StringComparison.Ordinal);
        return given >= 0 && when > given && then > when;
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXTEST001(EvaluationContext context) =>

        context
            .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .Where(predicate: (MethodDeclarationSyntax method) => method.TypeParameterList != null)
            .Select(
                selector: (MethodDeclarationSyntax method) =>
                    CreateAnalysisItem(
                        code: "STXTEST001",
                        description: "Tests and test helpers must not be generic.",
                        context: context,
                        location: method.GetLocation()
                    )
            );

    private static IEnumerable<AnalysisItem> EvaluateSTXTEST002(EvaluationContext context)
    {
        if (!IsTestSuite(context: context))
        {
            return Array.Empty<AnalysisItem>();
        }

        TypeDeclarationSyntax? declaration = context.Declarations.FirstOrDefault(
            predicate: (TypeDeclarationSyntax candidate) => candidate.BaseList != null
        );

        return !context.HasBaseClass
            ? Array.Empty<AnalysisItem>()
            :
            [
                CreateAnalysisItem(
                    code: "STXTEST002",
                    description: "Test suites must not inherit from base test classes.",
                    context: context,
                    location: declaration?.BaseList?.GetLocation()
                ),
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXTEST003(EvaluationContext context)
    {
        if (!IsTestSuite(context: context))
        {
            return Array.Empty<AnalysisItem>();
        }

        return context.TypeName.EndsWith(value: "Tests", comparisonType: StringComparison.Ordinal)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: "STXTEST003",
                    description: "A test suite must be named for its target type using the {TargetType}Tests convention.",
                    context: context,
                    location: context.Declarations[index: 0].Identifier.GetLocation()
                ),
            };
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXTEST004(EvaluationContext context)
    {
        if (!IsTestSuite(context: context))
        {
            return Array.Empty<AnalysisItem>();
        }

        return (
            !context.Declarations.Any(
                predicate: (TypeDeclarationSyntax declaration) =>
                    !declaration.Modifiers.Any(
                        predicate: (SyntaxToken modifier) => modifier.IsKind(kind: SyntaxKind.PartialKeyword)
                    )
            )
        )
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: "STXTEST004",
                    description: "Every declaration of a test suite must be partial.",
                    context: context,
                    location: context
                        .Declarations.First(
                            predicate: (TypeDeclarationSyntax declaration) =>
                                !declaration.Modifiers.Any(
                                    predicate: (SyntaxToken modifier) =>
                                        modifier.IsKind(kind: SyntaxKind.PartialKeyword)
                                )
                        )
                        .Identifier.GetLocation()
                ),
            };
    }

    private static bool IsTestSuite(EvaluationContext context) =>

        context.TypeName.EndsWith(value: "Tests", comparisonType: StringComparison.Ordinal)
        || context
            .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .Any(predicate: method =>
                method
                    .AttributeLists.SelectMany(selector: attributes => attributes.Attributes)
            .Any(predicate: attribute =>
                    {
                        string attributeName = attribute.Name.ToString();
                        return attributeName is "Fact" or "FactAttribute" or "Theory" or "TheoryAttribute";
                    })
            );
}