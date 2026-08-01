// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Exposures;
using cCoder.CodeAnalysis.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace cCoder.CodeAnalysis.Tests.Models;

public sealed class CodeAnalysisProjectFormattingComplianceTests
{
    [Fact]
    public void CodeAnalysisProjectShouldSeparateBlocksAndWrappedStatements()
    {
        string sourceDirectory = FindSourceDirectory();
        string projectPath = Path.Combine(
            sourceDirectory,
            "cCoder.CodeAnalysis",
            "cCoder.CodeAnalysis.csproj");
        ServiceCollection services = new();
        services.AddCodeAnalysis();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IArchitectureBuilder architectureBuilder =
            serviceProvider.GetRequiredService<IArchitectureBuilder>();

        Architecture architecture = architectureBuilder.Generate(projectPath);

        architecture.AnalysisItems
            .Where(item => item.Code == "STXFORMAT003"
                || item.Code == "STXFORMAT008")
            .Should().BeEmpty(
                "the CodeAnalysis implementation must comply with its block and wrapped-statement spacing rules");
    }

    private static string FindSourceDirectory()
    {
        for (
            DirectoryInfo? directory = new(AppContext.BaseDirectory);
            directory is not null;
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
