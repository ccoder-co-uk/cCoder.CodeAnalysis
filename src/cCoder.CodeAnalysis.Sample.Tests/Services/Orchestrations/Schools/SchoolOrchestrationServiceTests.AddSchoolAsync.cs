// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Schools;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.Schools;

public sealed partial class SchoolOrchestrationServiceTests
{
    [Fact]
    public async Task AddSchoolAsyncShouldPersistAndRaiseEventOnHappyPath()
    {
        // Given
        // When
        // Then
        School newSchool = new School { Id = 7 };

        schoolServiceMock
            .Setup(expression: (ISchoolService schoolService) => schoolService.AddSchoolAsync(newSchool: It.IsAny<School>()))
            .Returns(valueFunction: () => ValueTask.FromResult(result: new School { Id = 7 }));

        eventServiceMock
            .Setup(
expression: (IEntityEventService entityEventService) =>
                    entityEventService.RaiseAddEventAsync(entityName: "newSchool", entity: newSchool)
            )
            .Returns(value: ValueTask.CompletedTask);

        SchoolOrchestrationService service = CreateSchoolOrchestrationService();

        ((object)(await service.AddSchoolAsync(newSchool: newSchool))).Should()
            .BeSameAs(expected: newSchool, because: "");

        eventServiceMock.Verify(
expression: (IEntityEventService eventService) => eventService.RaiseAddEventAsync(entityName: "newSchool", entity: newSchool),
times: Times.Once
        );
    }
}