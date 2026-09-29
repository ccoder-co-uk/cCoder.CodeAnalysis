// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using System.Collections.Immutable;
using cCoder.CodeAnalysis.Exposures;
using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        context.RegisterSemanticModelAction(action: AnalyzeUsingDirectiveBoundaries);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        if (!(context.Compilation is CSharpCompilation compilation))
        {
            return;
        }

        AnalyzeProjectSourceBoundaries(
            context: context,
            compilation: compilation);

        AnalyzeGlobalUsingBoundaries(
            context: context,
            compilation: compilation);

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

    private static void AnalyzeProjectSourceBoundaries(
        CompilationAnalysisContext context,
        CSharpCompilation compilation)
    {
        if (!context.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(
            key: "build_property.MSBuildProjectDirectory",
            value: out string? projectDirectory)
            || string.IsNullOrWhiteSpace(value: projectDirectory))
        {
            return;
        }

        string projectRoot = EnsureTrailingDirectorySeparator(
            path: Path.GetFullPath(path: projectDirectory));

        StringComparison pathComparison = Path.DirectorySeparatorChar == '\\'
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
        {
            if (string.IsNullOrWhiteSpace(value: syntaxTree.FilePath))
            {
                continue;
            }

            string sourcePath = Path.GetFullPath(path: syntaxTree.FilePath);

            if (sourcePath.StartsWith(
                value: projectRoot,
                comparisonType: pathComparison))
            {
                continue;
            }

            DiagnosticDescriptor descriptor = Descriptors[key: "STXSTRUCT004"];

            context.ReportDiagnostic(
                diagnostic: Diagnostic.Create(
                    descriptor: descriptor,
                    location: syntaxTree.GetRoot(context.CancellationToken).GetLocation(),
                    $"Source file '{sourcePath}' is compiled from outside project directory "
                        + $"'{projectDirectory}'. Every project must own its source files."));
        }
    }

    private static string EnsureTrailingDirectorySeparator(string path) =>
        path.EndsWith(
            value: Path.DirectorySeparatorChar.ToString(),
            comparisonType: StringComparison.Ordinal)
            ? path
            : path + Path.DirectorySeparatorChar;

    private static void AnalyzeGlobalUsingBoundaries(
        CompilationAnalysisContext context,
        CSharpCompilation compilation)
    {
        if (context.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(
            key: "build_property.ImplicitUsings",
            value: out string? implicitUsings)
            && IsEnabled(value: implicitUsings))
        {
            DiagnosticDescriptor descriptor = Descriptors[key: "STXSTRUCT005"];

            context.ReportDiagnostic(
                diagnostic: Diagnostic.Create(
                    descriptor: descriptor,
                    location: Location.None,
                    "Implicit usings are not permitted. Each source file must declare "
                        + "the namespaces it consumes."));
        }

        foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
        {
            CompilationUnitSyntax compilationUnit =
                (CompilationUnitSyntax)syntaxTree.GetRoot(
                    cancellationToken: context.CancellationToken);

            UsingDirectiveSyntax? globalUsing = compilationUnit.Usings
                .FirstOrDefault(usingDirective => usingDirective.GlobalKeyword.IsKind(
                    kind: SyntaxKind.GlobalKeyword));

            bool isGlobalUsingsFile = string.Equals(
                a: Path.GetFileName(path: syntaxTree.FilePath),
                b: "GlobalUsings.cs",
                comparisonType: StringComparison.OrdinalIgnoreCase);

            if (globalUsing is null && !isGlobalUsingsFile)
            {
                continue;
            }

            DiagnosticDescriptor descriptor = Descriptors[key: "STXSTRUCT005"];
            Location location = globalUsing?.GetLocation()
                ?? compilationUnit.GetLocation();

            context.ReportDiagnostic(
                diagnostic: Diagnostic.Create(
                    descriptor: descriptor,
                    location: location,
                    "Global usings are not permitted. Each source file must declare "
                        + "the namespaces it consumes."));
        }
    }

    private static bool IsEnabled(string value) =>
        string.Equals(
            a: value,
            b: "enable",
            comparisonType: StringComparison.OrdinalIgnoreCase)
        || string.Equals(
            a: value,
            b: "true",
            comparisonType: StringComparison.OrdinalIgnoreCase);

    private static void AnalyzeUsingDirectiveBoundaries(
        SemanticModelAnalysisContext context)
    {
        DiagnosticDescriptor descriptor = Descriptors[key: "STXFORMAT014"];

        foreach (Diagnostic diagnostic in context.SemanticModel.GetDiagnostics(
            cancellationToken: context.CancellationToken)
            .Where(diagnostic => diagnostic.Id == "CS8019"))
        {
            context.ReportDiagnostic(
                diagnostic: Diagnostic.Create(
                    descriptor: descriptor,
                    location: diagnostic.Location,
                    "Using directive is unnecessary. Source files must import only "
                        + "the namespaces they consume."));
        }
    }

    private static Location FindLocation(CSharpCompilation compilation, AnalysisItem analysisItem)
    {
        if (!string.IsNullOrEmpty(value: analysisItem.FilePath))
        {
            SyntaxTree? sourceTree = compilation.SyntaxTrees.FirstOrDefault(
                predicate: tree => string.Equals(
                    a: tree.FilePath,
                    b: analysisItem.FilePath,
                    comparisonType: StringComparison.Ordinal));

            if (sourceTree is null || analysisItem.LineNumber <= 0
                || analysisItem.LineNumber > sourceTree.GetText().Lines.Count)
            {
                return Location.None;
            }

            return Location.Create(
                syntaxTree: sourceTree,
                textSpan: sourceTree.GetText().Lines[index: analysisItem.LineNumber - 1].Span);
        }

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
