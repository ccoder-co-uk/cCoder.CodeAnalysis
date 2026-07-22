// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Brokers.Files;
using cCoder.CodeAnalysis.Services.Foundations.Projects;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Tests.Services.Foundations.Projects;

public sealed class ProjectServiceTests
{
    private readonly Mock<IFileBroker> fileBrokerMock = new Mock<IFileBroker>();

    [Fact]
    public void ResolveProjectFilePathShouldReturnSuppliedProjectFile()
    {
        string projectFilePath = $"C:\\Projects\\{Guid.NewGuid()}.csproj";
        fileBrokerMock.Setup((IFileBroker broker) => broker.FileExists(projectFilePath)).Returns(value: true);
        ProjectService service = new ProjectService(fileBrokerMock.Object);
        string actualPath = service.ResolveProjectFilePath(projectFilePath);
        actualPath.Should().Be(projectFilePath, "");
        fileBrokerMock.Verify((IFileBroker broker) => broker.FileExists(projectFilePath), Times.Once);
        fileBrokerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void ResolveProjectFilePathShouldReturnProjectInsideDirectory()
    {
        string directoryPath = $"C:\\Projects\\{Guid.NewGuid()}";
        string projectFilePath = Path.Combine(directoryPath, "Example.csproj");
        fileBrokerMock.Setup((IFileBroker broker) => broker.FileExists(directoryPath)).Returns(value: false);
        fileBrokerMock.Setup((IFileBroker broker) => broker.DirectoryExists(directoryPath)).Returns(value: true);
        fileBrokerMock.Setup((IFileBroker broker) => broker.GetProjectFiles(directoryPath)).Returns([projectFilePath]);
        ProjectService service = new ProjectService(fileBrokerMock.Object);
        string actualPath = service.ResolveProjectFilePath(directoryPath);
        actualPath.Should().Be(projectFilePath, "");
        fileBrokerMock.Verify((IFileBroker broker) => broker.FileExists(directoryPath), Times.Once);
        fileBrokerMock.Verify((IFileBroker broker) => broker.DirectoryExists(directoryPath), Times.Once);
        fileBrokerMock.Verify((IFileBroker broker) => broker.GetProjectFiles(directoryPath), Times.Once);
        fileBrokerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void ResolveProjectFilePathShouldRejectDirectoryWithMultipleProjects()
    {
        string directoryPath = $"C:\\Projects\\{Guid.NewGuid()}";
        fileBrokerMock.Setup((IFileBroker broker) => broker.FileExists(directoryPath)).Returns(value: false);
        fileBrokerMock.Setup((IFileBroker broker) => broker.DirectoryExists(directoryPath)).Returns(value: true);
        fileBrokerMock
            .Setup((IFileBroker broker) => broker.GetProjectFiles(directoryPath))
            .Returns([Path.Combine(directoryPath, "One.csproj"), Path.Combine(directoryPath, "Two.csproj")]);
        ProjectService service = new ProjectService(fileBrokerMock.Object);
        Action resolve = delegate
        {
            service.ResolveProjectFilePath(directoryPath);
        };
        resolve.Should().Throw<InvalidOperationException>("", Array.Empty<object>());
    }
}