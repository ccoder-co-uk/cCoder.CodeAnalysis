// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Models;

public sealed class EvaluationContext
{
    public string TypeName { get; set; } = string.Empty;

    public StandardElementType StandardElementType { get; set; }

    public int LineNumber { get; set; }

    public bool IsPublic { get; set; }

    public bool IsApiController { get; set; }

    public IReadOnlyList<TypeDeclarationSyntax> Declarations { get; set; } = Array.Empty<TypeDeclarationSyntax>();

    public IReadOnlyList<TypeDependency> Dependencies { get; set; } = Array.Empty<TypeDependency>();

    public IReadOnlyList<string> ImplementedInterfaces { get; set; } = Array.Empty<string>();

    public IReadOnlyList<string> PublicMethodNames { get; set; } = Array.Empty<string>();

    public IReadOnlyList<string> ContractMethodNames { get; set; } = Array.Empty<string>();

    public IReadOnlyList<int> PublicMethodCallLineNumbers { get; set; } = Array.Empty<int>();

    public IReadOnlyList<string> PublicApiModelTypes { get; set; } = Array.Empty<string>();
}