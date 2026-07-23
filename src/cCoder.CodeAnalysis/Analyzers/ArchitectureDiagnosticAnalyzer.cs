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
    private static readonly string[] RuleCodes = new string[84]
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
            (string code) => code,
            (string code) =>
                new DiagnosticDescriptor(
                    code,
                    "cCoder architecture rule",
                    "{0}",
                    "cCoder.CodeAnalysis",
                    DiagnosticSeverity.Warning,
                    true,
                    null,
                    $"https://ccoder.co.uk/Documentation/CodeAnalysis/{GetRulePrefix(code)}/{code}"
                ),
            StringComparer.Ordinal
        );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Descriptors.Values.ToImmutableArray();

    private static string GetRulePrefix(string code)
    {
        return new string(code.TakeWhile(char.IsLetter).ToArray());
    }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        if (!(context.Compilation is CSharpCompilation compilation))
        {
            return;
        }
        Architecture architecture = ArchitectureAnalysis.Generate(compilation);
        foreach (AnalysisItem analysisItem in architecture.AnalysisItems)
        {
            if (Descriptors.TryGetValue(analysisItem.Code, out DiagnosticDescriptor? descriptor))
            {
                Location location = FindLocation(compilation, analysisItem);
                context.ReportDiagnostic(Diagnostic.Create(descriptor, location, analysisItem.Description));
            }
        }
    }

    private static Location FindLocation(CSharpCompilation compilation, AnalysisItem analysisItem)
    {
        INamedTypeSymbol? type = compilation.GetTypeByMetadataName(analysisItem.Type);
        SyntaxTree? syntaxTree =
            type?.DeclaringSyntaxReferences.Select((SyntaxReference reference) => reference.SyntaxTree)
                .FirstOrDefault((SyntaxTree candidate) => candidate.GetText().Lines.Count >= analysisItem.LineNumber)
            ?? type?.DeclaringSyntaxReferences.FirstOrDefault()?.SyntaxTree;
        if (syntaxTree == null || analysisItem.LineNumber <= 0)
        {
            return Location.None;
        }
        SourceText sourceText = syntaxTree.GetText();
        int lineIndex = Math.Min(analysisItem.LineNumber - 1, sourceText.Lines.Count - 1);
        TextLine line = sourceText.Lines[lineIndex];
        return Location.Create(syntaxTree, new TextSpan(line.Start, line.Span.Length));
    }
}
