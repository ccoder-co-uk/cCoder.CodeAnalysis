// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Diagnostics;
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
        string projectPath = Path.Combine(projectDirectory, "cCoder.CodeAnalysis.SampleWeb.csproj");
        await BuildProjectAsync(projectPath);
        string architectureFilePath = Path.Combine(projectDirectory, "project.stxjson");
        Architecture = ArchitectureJsonSerializer.Deserialize(await File.ReadAllTextAsync(architectureFilePath));
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private static async Task BuildProjectAsync(string projectPath)
    {
        using Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "build \"" + projectPath + "\" --no-restore",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };
        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                "The sample web project failed to compile." + Environment.NewLine + output + error
            );
        }
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