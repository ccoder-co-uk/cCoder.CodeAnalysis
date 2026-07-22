// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Diagnostics;
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Foundations.Architectures;

namespace cCoder.CodeAnalysis.Tests.Fixtures;

public sealed class SampleArchitectureFixture : IAsyncLifetime
{
    public Architecture Architecture { get; private set; } = new Architecture();

    public string ArchitectureFilePath { get; private set; } = string.Empty;

    public string ArchitectureJson { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        string sourceDirectory = FindSourceDirectory();
        string sampleProjectDirectory = Path.Combine(sourceDirectory, "cCoder.CodeAnalysis.Sample");
        string sampleProjectPath = Path.Combine(sampleProjectDirectory, "cCoder.CodeAnalysis.Sample.csproj");
        await BuildSampleProjectAsync(sampleProjectPath);
        ArchitectureFilePath = Path.Combine(sampleProjectDirectory, "project.stxjson");
        ArchitectureJson = await File.ReadAllTextAsync(ArchitectureFilePath);
        Architecture = ArchitectureJsonSerializer.Deserialize(ArchitectureJson);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private static async Task BuildSampleProjectAsync(string sampleProjectPath)
    {
        using Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "build \"" + sampleProjectPath + "\" --no-restore",
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
                "The sample project failed to compile." + Environment.NewLine + output + error
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
            string solutionPath = Path.Combine(directory.FullName, "cCoder.CodeAnalysis.slnx");
            if (File.Exists(solutionPath))
            {
                return directory.FullName;
            }
        }
        throw new DirectoryNotFoundException("The solution source directory could not be found.");
    }
}