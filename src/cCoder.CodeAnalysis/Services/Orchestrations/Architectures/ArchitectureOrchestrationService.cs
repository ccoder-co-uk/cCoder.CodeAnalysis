// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Foundations.Architectures;
using cCoder.CodeAnalysis.Services.Foundations.Projects;

namespace cCoder.CodeAnalysis.Services.Orchestrations.Architectures;

internal sealed class ArchitectureOrchestrationService(
    IProjectService projectService,
    IArchitectureService architectureService)
        : IArchitectureOrchestrationService
{
    private readonly IProjectService projectService = projectService;

    private readonly IArchitectureService architectureService = architectureService;

    public Architecture Generate(string path)
    {
        string projectFilePath = projectService.ResolveProjectFilePath(path);
        return architectureService.Build(projectFilePath);
    }
}