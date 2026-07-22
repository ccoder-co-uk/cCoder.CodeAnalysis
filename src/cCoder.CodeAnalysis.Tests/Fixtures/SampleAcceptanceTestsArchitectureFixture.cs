// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Brokers.Files;
using cCoder.CodeAnalysis.Exposures;
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Foundations.Architectures;
using cCoder.CodeAnalysis.Services.Foundations.Projects;
using cCoder.CodeAnalysis.Services.Orchestrations.Architectures;

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
        ArchitectureBuilder architectureBuilder = new ArchitectureBuilder(
            new ArchitectureOrchestrationService(new ProjectService(new FileBroker()), new ArchitectureService())
        );
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