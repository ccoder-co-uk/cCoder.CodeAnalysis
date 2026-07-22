// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Exposures;
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Orchestrations.Architectures;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Tests.Exposures;

public sealed class ArchitectureBuilderTests
{
    [Fact]
    public void GenerateShouldDelegateToOrchestrationService()
    {
        string path = GetRandomPath();
        Architecture expectedArchitecture = new Architecture();
        Mock<IArchitectureOrchestrationService> orchestrationServiceMock =
            new Mock<IArchitectureOrchestrationService>();
        orchestrationServiceMock
            .Setup((IArchitectureOrchestrationService service) => service.Generate(path))
            .Returns(expectedArchitecture);
        ArchitectureBuilder builder = new ArchitectureBuilder(orchestrationServiceMock.Object);
        Architecture actualArchitecture = builder.Generate(path);
        ((object)actualArchitecture).Should().BeSameAs(expectedArchitecture, "");
        orchestrationServiceMock.Verify(
            (IArchitectureOrchestrationService service) => service.Generate(path),
            Times.Once
        );
        orchestrationServiceMock.VerifyNoOtherCalls();
    }

    private static string GetRandomPath()
    {
        return $"C:\\Projects\\{Guid.NewGuid()}";
    }
}