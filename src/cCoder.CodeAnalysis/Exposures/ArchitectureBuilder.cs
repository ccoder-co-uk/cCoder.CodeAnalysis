// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Orchestrations.Architectures;

namespace cCoder.CodeAnalysis.Exposures;

internal sealed class ArchitectureBuilder(IArchitectureOrchestrationService architectureOrchestrationService)
    : IArchitectureBuilder
{
    private readonly IArchitectureOrchestrationService architectureOrchestrationService =
        architectureOrchestrationService;

    public Architecture Generate(string path)
    {
        return architectureOrchestrationService.Generate(path: path);
    }
}