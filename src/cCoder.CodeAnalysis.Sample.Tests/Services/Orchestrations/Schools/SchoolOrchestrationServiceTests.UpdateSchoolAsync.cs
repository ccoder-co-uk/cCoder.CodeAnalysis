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
    public async Task UpdateSchoolAsyncShouldPersistAndRaiseEventOnHappyPath()
    {
        // Given
        // When
        // Then
        School updatedSchool = CreateSchool();

        schoolServiceMock
            .Setup(expression:(ISchoolService schoolService) => schoolService.UpdateSchoolAsync(updatedSchool:It.IsAny<School>()))
            .Returns(valueFunction:() => ValueTask.FromResult(result:CreateSchool()));

        eventServiceMock
            .Setup(
expression:                (IEntityEventService entityEventService) =>
                    entityEventService.RaiseUpdateEventAsync(entityName:"updatedSchool", entity:updatedSchool)
            )
            .Returns(value:ValueTask.CompletedTask);

        SchoolOrchestrationService service = CreateSchoolOrchestrationService();

        ((object)(await service.UpdateSchoolAsync(updatedSchool:updatedSchool))).Should()
            .BeSameAs(expected:updatedSchool, because:"");

        eventServiceMock.Verify(
expression:            (IEntityEventService eventService) => eventService.RaiseUpdateEventAsync(entityName:"updatedSchool", entity:updatedSchool),
times:            Times.Once
        );
    }
}