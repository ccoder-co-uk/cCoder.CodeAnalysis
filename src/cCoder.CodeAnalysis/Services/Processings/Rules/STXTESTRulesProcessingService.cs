// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXTESTRulesProcessingService : ISTXTESTRulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        if (context.ArchitectureElement?.AnalysisTypeFacts is null)
        {
            yield break;
        }

        foreach (AnalysisItem item in EvaluateSTXTEST001(context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXTEST002(context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXTEST003(context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXTEST004(context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXTEST005(context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXTEST006(context))
        {
            yield return item;
        }
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
            && !context.TypeName.EndsWith("Tests", StringComparison.Ordinal)
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
            || !context.TypeName.EndsWith("ControllerAcceptanceTests", StringComparison.Ordinal)
            || context.TypeName.EndsWith("ImportControllerAcceptanceTests", StringComparison.Ordinal))
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
        context.TypeName.EndsWith("Tests", StringComparison.Ordinal)
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
            Type = context.TypeName,
            LineNumber = lineNumber == 0 ? context.LineNumber : lineNumber,
        };
}
