// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

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

        // When
        Architecture architecture = ArchitectureJsonSerializer.Deserialize(
            await File.ReadAllTextAsync(path: architecturePath));

        // Then
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
