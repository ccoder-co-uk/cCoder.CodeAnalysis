// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using System.Collections.Immutable;
using cCoder.CodeAnalysis.Exposures;
using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace cCoder.CodeAnalysis.Analyzers;

[DiagnosticAnalyzer("C#", new string[] { })]
public sealed class ArchitectureDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private static readonly ImmutableDictionary<string, DiagnosticDescriptor> Descriptors =
        DiagnosticCodeStandardPageIndex
            .GetDiagnosticCodeStandardPages()
            .ToImmutableDictionary<DiagnosticCodeStandardPage, string, DiagnosticDescriptor>(
            keySelector: (DiagnosticCodeStandardPage page) => page.DiagnosticCode,
            elementSelector: (DiagnosticCodeStandardPage page) =>
                new DiagnosticDescriptor(
                    id: page.DiagnosticCode,
                    title: "cCoder architecture rule",
                    messageFormat: "{0}",
                    category: "cCoder.CodeAnalysis",
                    defaultSeverity: DiagnosticSeverity.Warning,
                    isEnabledByDefault: true,
                    description: null,
                    helpLinkUri:
                        $"https://ccoder.co.uk/Documentation/CodeAnalysis/{GetRulePrefix(code: page.DiagnosticCode)}/{page.DiagnosticCode}"
                ),
            keyComparer: StringComparer.Ordinal
        );
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Descriptors.Values.ToImmutableArray();

    private static string GetRulePrefix(string code)
    {
        return new string(value: code.TakeWhile(predicate: char.IsLetter)
            .ToArray());
    }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(analysisMode: GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(action: AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        if (!(context.Compilation is CSharpCompilation compilation))
        {
            return;
        }

        Architecture architecture = ArchitectureAnalysis.Generate(compilation: compilation);

        foreach (AnalysisItem analysisItem in architecture.AnalysisItems)
        {
            if (Descriptors.TryGetValue(key: analysisItem.Code, value: out DiagnosticDescriptor? descriptor))
            {
                Location location = FindLocation(compilation: compilation, analysisItem: analysisItem);

                context.ReportDiagnostic(
                    diagnostic: Diagnostic.Create(descriptor: descriptor, location: location, analysisItem.Description)
                );
            }
        }
    }

    private static Location FindLocation(CSharpCompilation compilation, AnalysisItem analysisItem)
    {
        INamedTypeSymbol? type = compilation.GetTypeByMetadataName(fullyQualifiedMetadataName: analysisItem.Type);

        SyntaxTree? syntaxTree =
            type?.DeclaringSyntaxReferences.Select(selector: (SyntaxReference reference) => reference.SyntaxTree)
            .FirstOrDefault(
                    predicate: (SyntaxTree candidate) => candidate.GetText().Lines.Count >= analysisItem.LineNumber
                )
            ?? type?.DeclaringSyntaxReferences.FirstOrDefault()?.SyntaxTree;

        if (syntaxTree == null || analysisItem.LineNumber <= 0)
        {
            return Location.None;
        }

        SourceText sourceText = syntaxTree.GetText();
        int lineIndex = Math.Min(val1: analysisItem.LineNumber - 1, val2: sourceText.Lines.Count - 1);
        TextLine line = sourceText.Lines[index: lineIndex];

        return Location.Create(
            syntaxTree: syntaxTree,
            textSpan: new TextSpan(start: line.Start, length: line.Span.Length)
        );
    }
}