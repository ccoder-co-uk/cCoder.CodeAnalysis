// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXTESTRulesProcessingService : ISTXTESTRulesProcessingService
{
    private static readonly IArchitectureModelQueriesProcessingService architectureModelQueries =
        new ArchitectureModelQueriesProcessingService();

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        if (context.ArchitectureElement?.AnalysisTypeFacts is null)
        {
            return [];
        }

        return EvaluateSTXTEST001(context: context)
            .Concat(second: EvaluateSTXTEST002(context: context))
            .Concat(second: EvaluateSTXTEST003(context: context))
            .Concat(second: EvaluateSTXTEST004(context: context))
            .Concat(second: EvaluateSTXTEST005(context: context))
            .Concat(second: EvaluateSTXTEST006(context: context));
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXTEST001(
        EvaluationContext context) =>
        GetFacts(context).Methods
            .Where(method => method.IsGeneric)
            .Select(method => Create(
                "STXTEST001",
                "Tests and test helpers must not be generic.",
                context,
                method.LineNumber));

    private static IEnumerable<AnalysisItem> EvaluateSTXTEST002(
        EvaluationContext context)
    {
        TypeAnalysisFacts facts = GetFacts(context);

        return IsTestSuite(context, facts) && facts.BaseTypeLine > 0
            ? [Create(
                "STXTEST002",
                "Test suites must not inherit from base test classes.",
                context,
                facts.BaseTypeLine)]
            : [];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXTEST003(
        EvaluationContext context)
    {
        TypeAnalysisFacts facts = GetFacts(context);

        return IsTestSuite(context, facts)
            && !architectureModelQueries.GetTypeName(context).EndsWith("Tests", StringComparison.Ordinal)
                ? [Create(
                    "STXTEST003",
                    "A test suite must be named for its target type using the {TargetType}Tests convention.",
                    context)]
                : [];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXTEST004(
        EvaluationContext context)
    {
        TypeAnalysisFacts facts = GetFacts(context);

        return IsTestSuite(context, facts) && !facts.AllDeclarationsArePartial
            ? [Create(
                "STXTEST004",
                "Every declaration of a test suite must be partial.",
                context,
                facts.FirstNonPartialDeclarationLine)]
            : [];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXTEST005(
        EvaluationContext context) =>
        GetFacts(context).Methods
            .Where(method => method.IsTest && !method.HasGivenWhenThenComments)
            .Select(method => Create(
                "STXTEST005",
                "Every test must explicitly separate its Given, When, and Then phases with comments.",
                context,
                method.LineNumber));

    private static IEnumerable<AnalysisItem> EvaluateSTXTEST006(
        EvaluationContext context)
    {
        TypeAnalysisFacts facts = GetFacts(context);

        if (!IsTestSuite(context, facts)
            || !architectureModelQueries.GetTypeName(context).EndsWith("ControllerAcceptanceTests", StringComparison.Ordinal)
            || architectureModelQueries.GetTypeName(context).EndsWith("ImportControllerAcceptanceTests", StringComparison.Ordinal))
        {
            return [];
        }

        string[] requiredOperations = ["Get", "Post", "Put", "Delete"];
        bool coversCrud = requiredOperations.All(operation => facts.Methods.Any(
            method => method.IsFact
                && method.Name.StartsWith(operation, StringComparison.Ordinal)));

        return coversCrud
            ? []
            : [Create(
                "STXTEST006",
                "An API acceptance suite must cover Get, Post, Put, and Delete operations.",
                context)];
    }

    private static TypeAnalysisFacts GetFacts(EvaluationContext context) =>
        context.ArchitectureElement?.AnalysisTypeFacts ?? new TypeAnalysisFacts();

    private static bool IsTestSuite(
        EvaluationContext context,
        TypeAnalysisFacts facts) =>
        architectureModelQueries.GetTypeName(context).EndsWith("Tests", StringComparison.Ordinal)
        || facts.Methods.Any(method => method.IsTest);

    private static AnalysisItem Create(
        string code,
        string description,
        EvaluationContext context,
        int lineNumber = 0) => new()
        {
            Code = code,
            Description = description,
            Severity = AnalysisSeverity.Warning,
            Type = architectureModelQueries.GetTypeName(context),
            LineNumber = lineNumber == 0
                ? architectureModelQueries.GetLineNumber(context)
                : lineNumber,
        };
}
