// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Orchestrations.Architectures;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;

namespace cCoder.CodeAnalysis.Analyzers;

public static class ArchitectureAnalysis
{
    public static Architecture Generate(CSharpCompilation compilation)
    {
        ServiceCollection services = new ServiceCollection();
        services.AddCodeAnalysis();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        IArchitectureOrchestrationService architectureOrchestrationService =
            serviceProvider.GetRequiredService<IArchitectureOrchestrationService>();

        return architectureOrchestrationService.Generate(compilation: compilation);
    }
}