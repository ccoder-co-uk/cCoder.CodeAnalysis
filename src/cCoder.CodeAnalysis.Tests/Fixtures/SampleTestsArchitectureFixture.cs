// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Diagnostics;
using cCoder.CodeAnalysis.Exposures;
using cCoder.CodeAnalysis.Models;
using Microsoft.Extensions.DependencyInjection;

namespace cCoder.CodeAnalysis.Tests.Fixtures;

public sealed class SampleTestsArchitectureFixture : IAsyncLifetime
{
    public Architecture Architecture { get; private set; } = new Architecture();

    public string ArchitectureFilePath { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        string sourceDirectory = FindSourceDirectory();
        string projectPath = Path.Combine(
            sourceDirectory,
            "cCoder.CodeAnalysis.Sample.Tests",
            "cCoder.CodeAnalysis.Sample.Tests.csproj"
        );
        ArchitectureFilePath = Path.Combine(Path.GetDirectoryName(projectPath)!, "project.stxjson");
        await BuildProjectAsync(projectPath);
        ServiceCollection services = new ServiceCollection();
        services.AddCodeAnalysis();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IArchitectureBuilder architectureBuilder = serviceProvider.GetRequiredService<IArchitectureBuilder>();
        Architecture = architectureBuilder.Generate(projectPath);
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
                "The sample test project failed to compile." + Environment.NewLine + output + error
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
