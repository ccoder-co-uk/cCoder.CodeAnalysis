// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Diagnostics;
using cCoder.CodeAnalysis.BuildTasks;
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Foundations.Architectures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.BuildTasks;

public sealed class ArchitectureBuildTaskStatusCodeTests
{
    [Fact]
    public async Task AppControllerStatusCodes_WhenGeneratedByBuildTask_AreEmitted()
    {
        // Given
        string projectDirectory = FindFixtureDirectory();
        string architecturePath = Path.Combine(projectDirectory, "project.stxjson");
        File.Delete(path: architecturePath);

        // When
        using Process process = StartFixtureBuild(
            projectDirectory: projectDirectory);

        await process.WaitForExitAsync();

        Architecture architecture = ArchitectureJsonSerializer.Deserialize(
            await File.ReadAllTextAsync(path: architecturePath));

        // Then
        process.ExitCode.Should().Be(0);

        Class controller = architecture.Classes.Single(element =>
            element.Name == "StatusCodeProject.Controllers.AppController");

        controller.Methods.Single(method => method.Name == "GetIsAdmin")
            .HttpResponses.Select(response => response.StatusCode)
            .Should().Contain([200, 403, 500]);
        controller.Methods.Single(method => method.Name == "Post")
            .HttpResponses.Select(response => response.StatusCode)
            .Should().Contain([201, 403, 500]);
        architecture.AnalysisItems.Should().NotContain(item =>
            item.Type == controller.Name
            && (item.Code == "STXAPI005"
                || item.Code == "ODATA0001"
                || item.Code == "RFC0001"));
    }

    private static Process StartFixtureBuild(string projectDirectory)
    {
        string configuration = new DirectoryInfo(AppContext.BaseDirectory)
            .Parent!
            .Parent!
            .Name;

        ProcessStartInfo startInfo = new(fileName: "dotnet")
        {
            UseShellExecute = false,
        };

        startInfo.ArgumentList.Add(item: "build");
        startInfo.ArgumentList.Add(item: Path.Combine(projectDirectory, "StatusCodeProject.csproj"));
        startInfo.ArgumentList.Add(item: "--no-restore");
        startInfo.ArgumentList.Add(item: "--configuration");
        startInfo.ArgumentList.Add(item: configuration);
        startInfo.ArgumentList.Add(item: "--verbosity");
        startInfo.ArgumentList.Add(item: "quiet");
        startInfo.ArgumentList.Add(item: "-p:RunStatusCodeArchitectureGeneration=true");
        startInfo.ArgumentList.Add(
            item: $"-p:cCoderCodeAnalysisBuildTaskAssembly={typeof(GenerateArchitectureTask).Assembly.Location}");

        return Process.Start(startInfo: startInfo)!;
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
                "BuildTasks",
                "Fixtures",
                "StatusCodeProject");

            if (File.Exists(Path.Combine(fixtureDirectory, "StatusCodeProject.csproj")))
            {
                return fixtureDirectory;
            }
        }

        throw new DirectoryNotFoundException("The status-code build-task fixture could not be found.");
    }
}
