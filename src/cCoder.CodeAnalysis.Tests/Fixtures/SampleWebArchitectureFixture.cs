// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Foundations.Architectures;

namespace cCoder.CodeAnalysis.Tests.Fixtures;

public sealed class SampleWebArchitectureFixture : IAsyncLifetime
{
    public Architecture Architecture { get; private set; } = new Architecture();

    public async Task InitializeAsync()
    {
        string sourceDirectory = FindSourceDirectory();
        string projectDirectory = Path.Combine(sourceDirectory, "cCoder.CodeAnalysis.SampleWeb");
        string architectureFilePath = Path.Combine(projectDirectory, "project.stxjson");
        Architecture = ArchitectureJsonSerializer.Deserialize(await File.ReadAllTextAsync(architectureFilePath));
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
            if (File.Exists(Path.Combine(directory.FullName, "cCoder.CodeAnalysis.slnx")))
            {
                return directory.FullName;
            }
        }
        throw new DirectoryNotFoundException("The solution source directory could not be found.");
    }
}
