// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Foundations.Architectures;
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
        ArchitectureService architectureService =
            (ArchitectureService)serviceProvider.GetRequiredService<IArchitectureService>();

        return architectureService.Build(compilation);
    }
}
