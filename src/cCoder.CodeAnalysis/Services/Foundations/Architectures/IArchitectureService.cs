// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis.CSharp;

namespace cCoder.CodeAnalysis.Services.Foundations.Architectures;

internal interface IArchitectureService : ICodeAnalysisInfrastructureService
{
    ArchitectureBuild Build(string projectFilePath);
    ArchitectureBuild Build(CSharpCompilation compilation);
}