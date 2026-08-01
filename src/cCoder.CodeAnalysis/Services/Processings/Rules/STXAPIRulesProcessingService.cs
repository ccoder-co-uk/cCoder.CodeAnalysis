// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXAPIRulesProcessingService : ISTXAPIRulesProcessingService
{
    private static readonly IArchitectureModelQueriesProcessingService architectureModelQueries =
        new ArchitectureModelQueriesProcessingService();

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateSTXAPI001(context: context)
            .Concat(second: EvaluateSTXAPI002(context: context))
            .Concat(second: EvaluateSTXAPI003(context: context))
            .Concat(second: EvaluateSTXAPI004(context: context))
            .Concat(second: EvaluateSTXAPI005(context: context));
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
            Type = architectureModelQueries.GetTypeName(context),
            LineNumber = location is null
                ? architectureModelQueries.GetLineNumber(context)
                : location.GetLineSpan().StartLinePosition.Line + 1,
        };
    }

    private IEnumerable<AnalysisItem> EvaluateSTXAPI001(EvaluationContext context)
    {
        if (!architectureModelQueries.IsApiController(context: context))
        {
            return [];
        }

        int serviceDependencyCount = architectureModelQueries.GetDependencies(context: context).Count(
            predicate: (TypeDependency dependency) =>
                IsBusinessDependency(dependency.StandardElementType)
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

    private static bool IsBusinessDependency(StandardElementType elementType) =>
        elementType is StandardElementType.Exposure
            or StandardElementType.FoundationService
            or StandardElementType.ProcessingService
            or StandardElementType.OrchestrationService
            or StandardElementType.CoordinationService
            or StandardElementType.ManagementService
            or StandardElementType.AggregationService;

    private IEnumerable<AnalysisItem> EvaluateSTXAPI002(EvaluationContext context)
    {
        return !architectureModelQueries.IsApiController(context: context)
            || architectureModelQueries.GetPublicApiModelTypes(context: context).Count <= 1
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

    private IEnumerable<AnalysisItem> EvaluateSTXAPI003(EvaluationContext context)
    {
        string typeName = architectureModelQueries.GetTypeName(context).Split(separator: ['.'])
            .Last();

        return
            !architectureModelQueries.IsApiController(context: context)
            || typeName.EndsWith(value: "Controller", comparisonType: StringComparison.Ordinal)
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

    private IEnumerable<AnalysisItem> EvaluateSTXAPI004(EvaluationContext context)
    {
        if (!architectureModelQueries.IsApiController(context: context))
        {
            return [];
        }

        string[] verbs = ["Get", "Post", "Put", "Delete"];

        return architectureModelQueries.GetDeclarations(context)
            .SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
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

    private IEnumerable<AnalysisItem> EvaluateSTXAPI005(EvaluationContext context) =>
        !architectureModelQueries.IsApiController(context: context)
            && !architectureModelQueries.GetTypeName(context: context)
                .EndsWith(
                    value: "Middleware",
                    comparisonType: StringComparison.Ordinal)
            ? []
            : (context.ArchitectureElement?.Methods ?? [])
            .Where(method => method.IsHttpRequestHandler)
            .Where(method => !HasCompleteHttpOutcomeMapping(method: method))
            .Select(method => new AnalysisItem
            {
                Code = "STXAPI005",
                Description = "Every public HTTP handler must map success and caught failure paths to 2xx, 4xx, or 5xx responses.",
                Severity = AnalysisSeverity.Warning,
                Type = architectureModelQueries.GetTypeName(context: context),
                LineNumber = method.LineNumber,
            });

    private static bool HasCompleteHttpOutcomeMapping(Method method)
    {
        if (!method.HasTryCatch
            || !method.HttpResponses.Any(response => response.IsExceptionPath
                && response.StatusCode is >= 400 and <= 599)
            || method.HttpResponses.Any(response => response.StatusCode is < 200
                or >= 300 and <= 399
                or > 599))
        {
            return false;
        }

        return (method.IncomingExceptionTypes ?? [])
            .All(exceptionType => method.HttpResponses.Any(response =>
                response.IsExceptionPath
                && response.StatusCode is >= 400 and <= 599
                && (response.ExceptionType == exceptionType
                    || response.ExceptionType == "System.Exception")));
    }
}