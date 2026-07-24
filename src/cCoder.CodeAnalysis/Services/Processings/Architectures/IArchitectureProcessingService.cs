// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis.CSharp;

namespace cCoder.CodeAnalysis.Services.Processings.Architectures;

internal interface IArchitectureProcessingService : ICodeAnalysisInfrastructureService
{
    ArchitectureBuild Process(string path);
    ArchitectureBuild Process(CSharpCompilation compilation);
}