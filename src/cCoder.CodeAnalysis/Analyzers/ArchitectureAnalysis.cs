// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Foundations.Architectures;
using Microsoft.CodeAnalysis.CSharp;

namespace cCoder.CodeAnalysis.Analyzers;

public static class ArchitectureAnalysis
{
    public static Architecture Generate(CSharpCompilation compilation)
    {
        return new ArchitectureService().Build(compilation);
    }
}