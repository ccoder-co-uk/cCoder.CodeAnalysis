// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Diagnostics;
using cCoder.CodeAnalysis.Exposures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Exposures;

public sealed class RuntimeMarkerContractsTests
{
    [Fact]
    public void RuntimeMarkerContracts_WhenLoaded_DoNotRequireCodeAnalysisAssembly()
    {
        // Given
        Type[] runtimeMarkerContracts =
        [
            typeof(ICompositionExposure),
            typeof(IUtilityBroker)
        ];

        // When
        string[] markerAssemblies = runtimeMarkerContracts
            .Select(markerContract => markerContract.Assembly.GetName().Name!)
            .Distinct()
            .ToArray();

        // Then
        markerAssemblies.Should().Equal("cCoder.CodeAnalysis.Contracts");

        runtimeMarkerContracts[0].Assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Should().NotContain("cCoder.CodeAnalysis");
    }

    [Fact]
    public async Task RuntimeMarkerConsumer_WhenPublished_DoesNotLoadCodeAnalysisAssembly()
    {
        // Given
        string fixtureDirectory = FindFixtureDirectory();
        string contractsProject = Path.GetFullPath(Path.Combine(
            fixtureDirectory,
            "..",
            "..",
            "..",
            "..",
            "cCoder.CodeAnalysis.Contracts",
            "cCoder.CodeAnalysis.Contracts.csproj"));
        string testVersion = $"1.0.0-runtime{Guid.NewGuid():N}";
        string packageDirectory = Path.Combine(
            Path.GetTempPath(),
            $"CodeAnalysis-RuntimePackages-{Guid.NewGuid():N}");
        string publishDirectory = Path.Combine(
            Path.GetTempPath(),
            $"CodeAnalysis-RuntimeMarkerConsumer-{Guid.NewGuid():N}");

        try
        {
            // When
            ProcessResult packResult = await RunDotNetAsync(
                workingDirectory: fixtureDirectory,
                arguments:
                [
                    "pack",
                    contractsProject,
                    "--configuration",
                    "Release",
                    "--output",
                    packageDirectory,
                    $"-p:Version={testVersion}"
                ]);

            packResult.ExitCode.Should().Be(0, packResult.Output);

            ProcessResult restoreResult = await RunDotNetAsync(
                workingDirectory: fixtureDirectory,
                arguments:
                [
                    "restore",
                    "RuntimeMarkerConsumer.csproj",
                    "--source",
                    packageDirectory,
                    "--force",
                    "--no-cache",
                    $"-p:RuntimeContractsTestVersion={testVersion}"
                ]);

            restoreResult.ExitCode.Should().Be(0, restoreResult.Output);

            ProcessResult publishResult = await RunDotNetAsync(
                workingDirectory: fixtureDirectory,
                arguments:
                [
                    "publish",
                    "RuntimeMarkerConsumer.csproj",
                    "--configuration",
                    "Release",
                    "--output",
                    publishDirectory,
                    "--no-restore",
                    $"-p:RuntimeContractsTestVersion={testVersion}"
                ]);

            publishResult.ExitCode.Should().Be(0, publishResult.Output);

            ProcessResult executionResult = await RunDotNetAsync(
                workingDirectory: publishDirectory,
                arguments: ["RuntimeMarkerConsumer.dll"]);

            // Then
            executionResult.ExitCode.Should().Be(0, executionResult.Output);
            executionResult.Output.Should().Contain("RuntimeMarkerConsumer");
            executionResult.Output.Should().Contain("cCoder.CodeAnalysis.Contracts");
            File.Exists(Path.Combine(publishDirectory, "cCoder.CodeAnalysis.Contracts.dll"))
                .Should().BeTrue();
            File.Exists(Path.Combine(publishDirectory, "cCoder.CodeAnalysis.dll"))
                .Should().BeFalse();
        }
        finally
        {
            if (Directory.Exists(publishDirectory))
            {
                Directory.Delete(path: publishDirectory, recursive: true);
            }

            if (Directory.Exists(packageDirectory))
            {
                Directory.Delete(path: packageDirectory, recursive: true);
            }
        }
    }

    private static string FindFixtureDirectory()
    {
        for (DirectoryInfo? directory = new(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent)
        {
            string fixtureDirectory = Path.Combine(
                directory.FullName,
                "cCoder.CodeAnalysis.Tests",
                "Exposures",
                "Fixtures",
                "RuntimeMarkerConsumer");

            if (File.Exists(Path.Combine(fixtureDirectory, "RuntimeMarkerConsumer.csproj")))
            {
                return fixtureDirectory;
            }
        }

        throw new DirectoryNotFoundException(
            "The runtime-marker consumer fixture could not be found.");
    }

    private static async Task<ProcessResult> RunDotNetAsync(
        string workingDirectory,
        string[] arguments)
    {
        ProcessStartInfo startInfo = new(fileName: "dotnet")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using Process process = Process.Start(startInfo)!;
        string standardOutput = await process.StandardOutput.ReadToEndAsync();
        string standardError = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new ProcessResult(
            ExitCode: process.ExitCode,
            Output: standardOutput + standardError);
    }

    private sealed record ProcessResult(int ExitCode, string Output);
}