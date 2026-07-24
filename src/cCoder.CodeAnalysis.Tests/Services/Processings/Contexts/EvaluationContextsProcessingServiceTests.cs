// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Contexts;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Contexts;

public sealed partial class EvaluationContextsProcessingServiceTests
{
    private readonly EvaluationContextsProcessingService service = new();

    private static ArchitectureBuild CreateArchitectureBuild(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            text: source,
            path: "ExternalService.cs");

        MetadataReference[] references =
            ((string)AppContext.GetData(name: "TRUSTED_PLATFORM_ASSEMBLIES")!)
                .Split(separator: Path.PathSeparator)
                .Select(selector: path => MetadataReference.CreateFromFile(path: path))
                .ToArray();

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "ExternalServiceTests",
            syntaxTrees: [syntaxTree],
            references: references);

        INamedTypeSymbol[] declaredTypes = syntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax>()
            .Select(selector: declaration =>
                compilation.GetSemanticModel(syntaxTree: syntaxTree)
                    .GetDeclaredSymbol(declaration: declaration))
            .OfType<INamedTypeSymbol>()
            .ToArray();

        return new ArchitectureBuild
        {
            Compilation = compilation,
            DeclaredTypes = declaredTypes,
            ProjectLineEnding = Environment.NewLine,
        };
    }
}