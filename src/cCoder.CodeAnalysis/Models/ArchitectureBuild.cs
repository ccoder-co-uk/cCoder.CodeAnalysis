// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
#nullable disable
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace cCoder.CodeAnalysis.Models;

internal sealed class ArchitectureBuild
{
    public Architecture Architecture { get; set; }
    public CSharpCompilation Compilation { get; set; }
    public INamedTypeSymbol[] DeclaredTypes { get; set; }
    public string ProjectLineEnding { get; set; }
}