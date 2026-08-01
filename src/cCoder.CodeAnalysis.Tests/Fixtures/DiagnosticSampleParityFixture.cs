// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Exposures;
using cCoder.CodeAnalysis.Models;
using Microsoft.Extensions.DependencyInjection;

namespace cCoder.CodeAnalysis.Tests.Fixtures;

public sealed class DiagnosticSampleParityFixture : IAsyncLifetime
{
    public IReadOnlyDictionary<string, Architecture> Architectures { get; private set; } =
        new Dictionary<string, Architecture>();

    public Task InitializeAsync()
    {
        string sourceDirectory = FindSourceDirectory();
        ServiceCollection services = new ServiceCollection();
        services.AddCodeAnalysis();

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IArchitectureBuilder architectureBuilder =
            serviceProvider.GetRequiredService<IArchitectureBuilder>();

        string[] projectNames =
        [
            "cCoder.CodeAnalysis.Sample",
            "cCoder.CodeAnalysis.SampleWeb",
            "cCoder.CodeAnalysis.Sample.Tests",
            "cCoder.CodeAnalysis.Sample.AcceptanceTests",
            "School.Cli",
            "School.Cli.MissingHost",
            "School.Cli.BadHost",
        ];

        Architectures = projectNames.ToDictionary(
            keySelector: projectName => projectName,
            elementSelector: projectName => architectureBuilder.Generate(
                Path.Combine(sourceDirectory, projectName, $"{projectName}.csproj")),
            comparer: StringComparer.Ordinal);

        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static string FindSourceDirectory()
    {
        for (
            DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory != null;
            directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "cCoder.CodeAnalysis.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            "The solution source directory could not be found.");
    }
}
