// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Contexts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Contexts;

public sealed partial class EvaluationContextsProcessingServiceTests
{
    private readonly EvaluationContextsProcessingService service = new();

    private static ArchitectureBuild CreateArchitectureBuild(
        string source,
        params MetadataReference[] additionalReferences)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            text: source,
            path: "ExternalService.cs");

        MetadataReference[] references =
        [
            .. ((string)AppContext.GetData(name: "TRUSTED_PLATFORM_ASSEMBLIES")!)
                .Split(separator: Path.PathSeparator)
                .Select(selector: path => MetadataReference.CreateFromFile(path: path)),
            .. additionalReferences,
        ];

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

    private static MetadataReference CreateMetadataReference(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(text: source);

        MetadataReference[] references =
            ((string)AppContext.GetData(name: "TRUSTED_PLATFORM_ASSEMBLIES")!)
                .Split(separator: Path.PathSeparator)
                .Select(selector: path => MetadataReference.CreateFromFile(path: path))
                .ToArray();

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "ReferencedServices",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(
                outputKind: OutputKind.DynamicallyLinkedLibrary));

        using MemoryStream assemblyStream = new();
        compilation.Emit(peStream: assemblyStream);

        return MetadataReference.CreateFromImage(
            peImage: assemblyStream.ToArray());
    }
}