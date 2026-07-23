// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Exposures;
using cCoder.CodeAnalysis.Models;
using Microsoft.Extensions.DependencyInjection;

namespace cCoder.CodeAnalysis.Tests.Fixtures;

public sealed class SampleAcceptanceTestsArchitectureFixture : IAsyncLifetime
{
    public Architecture Architecture { get; private set; } = new Architecture();

    public Task InitializeAsync()
    {
        string projectPath = Path.Combine(
            FindSourceDirectory(),
            "cCoder.CodeAnalysis.Sample.AcceptanceTests",
            "cCoder.CodeAnalysis.Sample.AcceptanceTests.csproj"
        );
        ServiceCollection services = new ServiceCollection();
        services.AddCodeAnalysis();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IArchitectureBuilder architectureBuilder = serviceProvider.GetRequiredService<IArchitectureBuilder>();
        Architecture = architectureBuilder.Generate(projectPath);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private static string FindSourceDirectory()
    {
        for (
            DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory != null;
            directory = directory.Parent
        )
        {
            string solutionPath = Path.Combine(directory.FullName, "cCoder.CodeAnalysis.slnx");
            if (File.Exists(solutionPath))
            {
                return directory.FullName;
            }
        }
        throw new DirectoryNotFoundException("The solution source directory could not be found.");
    }
}
