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

    [Fact]
    public void SourceConsumerProjects_WhenLoadingAnalyzer_LoadCoreAssemblyFromDependencyCompleteOutput()
    {
        // Given
        string sourceDirectory = FindSourceDirectory();
        string expectedPath =
            @"..\cCoder.CodeAnalysis.Analyzers\bin\$(Configuration)\netstandard2.0\cCoder.CodeAnalysis.dll";
        string incompletePath =
            @"..\cCoder.CodeAnalysis\bin\$(Configuration)\netstandard2.0\cCoder.CodeAnalysis.dll";
        string[] projectPaths =
        [
            Path.Combine(sourceDirectory, "cCoder.CodeAnalysis.Sample", "cCoder.CodeAnalysis.Sample.csproj"),
            Path.Combine(sourceDirectory, "cCoder.CodeAnalysis.Sample.Tests", "cCoder.CodeAnalysis.Sample.Tests.csproj"),
        ];

        foreach (string projectPath in projectPaths)
        {
            // When
            string project = File.ReadAllText(path: projectPath);

            // Then
            project.Should().Contain(expectedPath, "analyzer dependencies must resolve beside its core assembly");
            project.Should().NotContain(incompletePath, "the separate core output does not contain analyzer dependencies");
        }
    }

    [Fact]
    public void SourceConsumerProjects_WhenLoadingAnalyzer_DeclareDependencyInjectionAssembliesWithoutEvaluationTimeGlobs()
    {
        // Given
        string sourceDirectory = FindSourceDirectory();
        string dependencyInjectionAssembly =
            @"..\cCoder.CodeAnalysis.Analyzers\bin\$(Configuration)\netstandard2.0\Microsoft.Extensions.DependencyInjection.dll";
        string dependencyInjectionAbstractionsAssembly =
            @"..\cCoder.CodeAnalysis.Analyzers\bin\$(Configuration)\netstandard2.0\Microsoft.Extensions.DependencyInjection.Abstractions.dll";
        string evaluationTimeGlob =
            @"..\cCoder.CodeAnalysis.Analyzers\bin\$(Configuration)\netstandard2.0\Microsoft.Extensions.DependencyInjection*.dll";
        string[] projectPaths =
        [
            Path.Combine(sourceDirectory, "cCoder.CodeAnalysis", "cCoder.CodeAnalysis.csproj"),
            Path.Combine(sourceDirectory, "cCoder.CodeAnalysis.Sample", "cCoder.CodeAnalysis.Sample.csproj"),
            Path.Combine(sourceDirectory, "cCoder.CodeAnalysis.Sample.Tests", "cCoder.CodeAnalysis.Sample.Tests.csproj"),
        ];

        foreach (string projectPath in projectPaths)
        {
            // When
            string project = File.ReadAllText(path: projectPath);

            // Then
            project.Should().Contain(
                dependencyInjectionAssembly,
                "a fresh build must retain the dependency item before the analyzer output exists"
            );
            project.Should().Contain(
                dependencyInjectionAbstractionsAssembly,
                "the analyzer requires the dependency-injection abstractions assembly"
            );
            project.Should().NotContain(
                evaluationTimeGlob,
                "MSBuild expands globs before a fresh build has produced the analyzer dependencies"
            );
        }
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
