// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXAPIRulesProcessingService : ISTXAPIRulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        foreach (AnalysisItem item in EvaluateSTXAPI001(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXAPI002(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXAPI003(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXAPI004(context: context))
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

    private static IEnumerable<AnalysisItem> EvaluateSTXAPI001(EvaluationContext context)
    {
        if (!context.IsApiController)
        {
            return [];
        }

        int serviceDependencyCount = context.Dependencies.Count(
            predicate: (TypeDependency dependency) =>
                dependency.StandardElementType
                    is >= StandardElementType.Exposure
                        and <= StandardElementType.AggregationService
                || dependency.TypeName.EndsWith(value: "Service", comparisonType: StringComparison.Ordinal)
        );

        return serviceDependencyCount == 1
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXAPI001",
                    description: "An API controller must have exactly one business dependency.",
                    context: context
                ),
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPI002(EvaluationContext context)
    {
        return !context.IsApiController || context.PublicApiModelTypes.Count <= 1
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXAPI002",
                    description: "An API controller must expose a single model contract.",
                    context: context
                ),
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPI003(EvaluationContext context)
    {
        string typeName = context.TypeName.Split(separator: ['.'])
            .Last();

        return
            !context.IsApiController || typeName.EndsWith(value: "Controller", comparisonType: StringComparison.Ordinal)
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXAPI003",
                    description: "An API controller class name must end with Controller.",
                    context: context
                ),
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPI004(EvaluationContext context)
    {
        if (!context.IsApiController)
        {
            return [];
        }

        string[] verbs = ["Get", "Post", "Put", "Delete"];

        return context
            .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
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
                        code: "STXAPI004",
                        description: "API controller methods must use HTTP nouns: Get, Post, Put, or Delete.",
                        context: context,
                        location: method.GetLocation()
                    )
            );
    }
}