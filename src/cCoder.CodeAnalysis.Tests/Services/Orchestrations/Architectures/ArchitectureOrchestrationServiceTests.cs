// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Foundations.Architectures;
using cCoder.CodeAnalysis.Services.Foundations.Projects;
using cCoder.CodeAnalysis.Services.Orchestrations.Architectures;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Tests.Services.Orchestrations.Architectures;

public sealed class ArchitectureOrchestrationServiceTests
{
    [Fact]
    public void GenerateShouldResolveProjectAndBuildArchitecture()
    {
        string suppliedPath = $"C:\\Projects\\{Guid.NewGuid()}";
        string projectFilePath = Path.Combine(suppliedPath, "Example.csproj");
        Architecture expectedArchitecture = new Architecture();
        Mock<IProjectService> projectServiceMock = new Mock<IProjectService>();
        Mock<IArchitectureService> architectureServiceMock = new Mock<IArchitectureService>();
        projectServiceMock
            .Setup((IProjectService projectService) => projectService.ResolveProjectFilePath(suppliedPath))
            .Returns(projectFilePath);
        architectureServiceMock
            .Setup((IArchitectureService architectureService) => architectureService.Build(projectFilePath))
            .Returns(expectedArchitecture);
        ArchitectureOrchestrationService service = new ArchitectureOrchestrationService(
            projectServiceMock.Object,
            architectureServiceMock.Object
        );
        Architecture actualArchitecture = service.Generate(suppliedPath);
        ((object)actualArchitecture).Should().BeSameAs(expectedArchitecture, "");
        projectServiceMock.Verify(
            (IProjectService projectService) => projectService.ResolveProjectFilePath(suppliedPath),
            Times.Once
        );
        architectureServiceMock.Verify(
            (IArchitectureService architectureService) => architectureService.Build(projectFilePath),
            Times.Once
        );
        projectServiceMock.VerifyNoOtherCalls();
        architectureServiceMock.VerifyNoOtherCalls();
    }
}