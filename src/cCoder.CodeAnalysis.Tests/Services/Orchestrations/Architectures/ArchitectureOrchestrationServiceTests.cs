// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Orchestrations.Architectures;
using cCoder.CodeAnalysis.Services.Processings.Architectures;
using cCoder.CodeAnalysis.Services.Processings.Contexts;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Tests.Services.Orchestrations.Architectures;

public sealed class ArchitectureOrchestrationServiceTests
{
    [Fact]
    public void GenerateShouldResolveProjectAndBuildArchitecture()
    {
        string suppliedPath = $"C:\\Projects\\{Guid.NewGuid()}";
        Architecture expectedArchitecture = new Architecture();
        ArchitectureBuild architectureBuild = new ArchitectureBuild
        {
            Architecture = expectedArchitecture,
        };
        Mock<IArchitectureProcessingService> architectureProcessingServiceMock =
            new Mock<IArchitectureProcessingService>();
        Mock<IEvaluationContextsProcessingService> evaluationContextsProcessingServiceMock =
            new Mock<IEvaluationContextsProcessingService>();
        Mock<IRuleEvaluationsProcessingService> ruleEvaluationsProcessingServiceMock =
            new Mock<IRuleEvaluationsProcessingService>();
        architectureProcessingServiceMock
            .Setup(
                (IArchitectureProcessingService architectureProcessingService) =>
                    architectureProcessingService.Process(suppliedPath)
            )
            .Returns(architectureBuild);
        evaluationContextsProcessingServiceMock
            .Setup(
                (IEvaluationContextsProcessingService service) =>
                    service.Process(architectureBuild)
            )
            .Returns([]);
        ruleEvaluationsProcessingServiceMock
            .Setup(
                (IRuleEvaluationsProcessingService service) =>
                    service.Process(It.IsAny<IEnumerable<EvaluationContext>>())
            )
            .Returns([]);
        ArchitectureOrchestrationService service = new ArchitectureOrchestrationService(
            architectureProcessingServiceMock.Object,
            evaluationContextsProcessingServiceMock.Object,
            ruleEvaluationsProcessingServiceMock.Object
        );
        Architecture actualArchitecture = service.Generate(suppliedPath);
        ((object)actualArchitecture).Should().BeSameAs(expectedArchitecture, "");
        architectureProcessingServiceMock.Verify(
            (IArchitectureProcessingService architectureProcessingService) =>
                architectureProcessingService.Process(suppliedPath),
            Times.Once
        );
        architectureProcessingServiceMock.VerifyNoOtherCalls();
    }
}