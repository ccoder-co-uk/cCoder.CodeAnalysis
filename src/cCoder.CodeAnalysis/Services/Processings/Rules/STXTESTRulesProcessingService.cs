// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXTESTRulesProcessingService : ISTXTESTRulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        TypeAnalysisFacts? facts = context.ArchitectureElement?.AnalysisTypeFacts;

        if (facts is null)
        {
            yield break;
        }

        bool isTestSuite = context.TypeName.EndsWith("Tests", StringComparison.Ordinal)
            || facts.Methods.Any(method => method.IsTest);

        foreach (MethodAnalysisFacts method in facts.Methods.Where(method => method.IsGeneric))
        {
            yield return Create("STXTEST001", "Tests and test helpers must not be generic.", context, method.LineNumber);
        }

        if (isTestSuite && facts.BaseTypeLine > 0)
        {
            yield return Create("STXTEST002", "Test suites must not inherit from base test classes.", context, facts.BaseTypeLine);
        }

        if (isTestSuite && !context.TypeName.EndsWith("Tests", StringComparison.Ordinal))
        {
            yield return Create("STXTEST003", "A test suite must be named for its target type using the {TargetType}Tests convention.", context);
        }

        if (isTestSuite && !facts.AllDeclarationsArePartial)
        {
            yield return Create("STXTEST004", "Every declaration of a test suite must be partial.", context, facts.FirstNonPartialDeclarationLine);
        }

        foreach (MethodAnalysisFacts method in facts.Methods.Where(
            method => method.IsTest && !method.HasGivenWhenThenComments))
        {
            yield return Create("STXTEST005", "Every test must explicitly separate its Given, When, and Then phases with comments.", context, method.LineNumber);
        }

        if (isTestSuite
            && context.TypeName.EndsWith("ControllerAcceptanceTests", StringComparison.Ordinal)
            && !context.TypeName.EndsWith("ImportControllerAcceptanceTests", StringComparison.Ordinal))
        {
            string[] requiredOperations = ["Get", "Post", "Put", "Delete"];

            if (!requiredOperations.All(operation => facts.Methods.Any(
                method => method.IsFact && method.Name.StartsWith(operation, StringComparison.Ordinal))))
            {
                yield return Create("STXTEST006", "An API acceptance suite must cover Get, Post, Put, and Delete operations.", context);
            }
        }
    }

    private static AnalysisItem Create(
        string code,
        string description,
        EvaluationContext context,
        int lineNumber = 0) =>
        new()
        {
            Code = code,
            Description = description,
            Severity = AnalysisSeverity.Warning,
            Type = context.TypeName,
            LineNumber = lineNumber == 0 ? context.LineNumber : lineNumber,
        };
}
