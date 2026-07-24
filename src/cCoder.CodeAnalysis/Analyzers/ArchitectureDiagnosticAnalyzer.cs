// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using System.Collections.Immutable;
using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace cCoder.CodeAnalysis.Analyzers;

[DiagnosticAnalyzer("C#", new string[] { })]
public sealed class ArchitectureDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private static readonly string[] RuleCodes = new string[90]
    {
        "STX0001",
        "STX0002",
        "STX0003",
        "STX0004",
        "STX0005",
        "STX0006",
        "STX0007",
        "STX0008",
        "STX0009",
        "STX0010",
        "STX0011",
        "STX0012",
        "STX0013",
        "STX0014",
        "STX0015",
        "STX0016",
        "STX0017",
        "STX0018",
        "STX0019",
        "STX0020",
        "STX0021",
        "STX0022",
        "STX0023",
        "STXAPP001",
        "STXAPP002",
        "STXAPP003",
        "STXAPP004",
        "STXAPP005",
        "STXAPP006",
        "STXAPP007",
        "STXAPP008",
        "STXAPP009",
        "STXA001",
        "STXA002",
        "STXAPI001",
        "STXAPI002",
        "STXAPI003",
        "STXAPI004",
        "STXB001",
        "STXB002",
        "STXB003",
        "STXB004",
        "STXB005",
        "STXB006",
        "STXB007",
        "STXC001",
        "STXC002",
        "STXD001",
        "STXD002",
        "STXE001",
        "STXE002",
        "STXE003",
        "STXE004",
        "STXE005",
        "STXEX001",
        "STXEX002",
        "STXEX003",
        "STXF001",
        "STXF002",
        "STXF003",
        "STXFORMAT001",
        "STXFORMAT002",
        "STXFORMAT003",
        "STXFORMAT004",
        "STXFORMAT005",
        "STXFORMAT006",
        "STXFORMAT007",
        "STXFORMAT008",
        "STXFORMAT009",
        "STXFORMAT010",
        "STXFORMAT011",
        "STXFORMAT012",
        "STXFORMAT013",
        "STXMG001",
        "STXMG002",
        "STXM001",
        "STXM002",
        "STXM003",
        "STXO001",
        "STXO002",
        "STXP001",
        "STXP002",
        "STXP003",
        "STXSTRUCT001",
        "STXTEST001",
        "STXTEST002",
        "STXTEST003",
        "STXTEST004",
        "STXTEST005",
        "STXTEST006",
    };
    private static readonly ImmutableDictionary<string, DiagnosticDescriptor> Descriptors =
        RuleCodes.ToImmutableDictionary<string, string, DiagnosticDescriptor>(
            keySelector: (string code) => code,
            elementSelector: (string code) =>
                new DiagnosticDescriptor(
                    id: code,
                    title: "cCoder architecture rule",
                    messageFormat: "{0}",
                    category: "cCoder.CodeAnalysis",
                    defaultSeverity: DiagnosticSeverity.Warning,
                    isEnabledByDefault: true,
                    description: null,
                    helpLinkUri: $"https://ccoder.co.uk/Documentation/CodeAnalysis/{GetRulePrefix(code: code)}/{code}"
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