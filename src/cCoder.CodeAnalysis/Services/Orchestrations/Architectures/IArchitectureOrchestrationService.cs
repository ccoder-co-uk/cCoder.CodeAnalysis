// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis.CSharp;

namespace cCoder.CodeAnalysis.Services.Orchestrations.Architectures;

internal interface IArchitectureOrchestrationService : ICodeAnalysisInfrastructureService
{
    Architecture Generate(string path);
    Architecture Generate(CSharpCompilation compilation);
}