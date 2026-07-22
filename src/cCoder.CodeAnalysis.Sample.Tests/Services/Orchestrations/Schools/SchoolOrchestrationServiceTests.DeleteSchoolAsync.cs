// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Schools;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.Schools;

public sealed partial class SchoolOrchestrationServiceTests
{
    [Fact]
    public async Task DeleteSchoolAsyncShouldDeleteAndRaiseEventWhenSchoolExists()
    {
        // Given
        // When
        // Then
        School existingSchool = new School { Id = 7 };

        schoolServiceMock.Setup(expression:(ISchoolService schoolService) => schoolService.GetSchool(schoolId:7))
            .Returns(value:existingSchool);

        schoolServiceMock
            .Setup(expression:(ISchoolService schoolService) => schoolService.DeleteSchoolAsync(schoolId:7))
            .Returns(value:ValueTask.CompletedTask);

        eventServiceMock
            .Setup(
expression:                (IEntityEventService entityEventService) =>
                    entityEventService.RaiseDeleteEventAsync(entityName:"updatedSchool", entity:existingSchool)
            )
            .Returns(value:ValueTask.CompletedTask);

        SchoolOrchestrationService service = CreateSchoolOrchestrationService();
        await service.DeleteSchoolAsync(schoolId:7);
        schoolServiceMock.Verify(expression:(ISchoolService foundation) => foundation.DeleteSchoolAsync(schoolId:7), times:Times.Once);
    }

    [Fact]
    public async Task DeleteSchoolAsyncShouldStopWhenSchoolDoesNotExist()
    {
        // Given
        // When
        // Then
        schoolServiceMock.Setup(expression:(ISchoolService schoolService) => schoolService.GetSchool(schoolId:7))
            .Returns(value:(School?)null);

        SchoolOrchestrationService service = CreateSchoolOrchestrationService();
        await service.DeleteSchoolAsync(schoolId:7);

        schoolServiceMock.Verify(
expression:            (ISchoolService foundation) => foundation.DeleteSchoolAsync(schoolId:It.IsAny<int>()),
times:            Times.Never
        );
    }
}