// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Teachers;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Teachers;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.Teachers;

public sealed partial class TeacherOrchestrationServiceTests
{
    [Fact]
    public async Task UpdateTeacherAsyncShouldPersistAndRaiseEventOnHappyPath()
    {
        // Given
        // When
        // Then
        Teacher updatedTeacher = CreateTeacher();

        teacherServiceMock
            .Setup(expression:(ITeacherService teacherService) => teacherService.UpdateTeacherAsync(updatedTeacher:It.IsAny<Teacher>()))
            .Returns(valueFunction:() => ValueTask.FromResult(result:CreateTeacher()));

        eventServiceMock
            .Setup(
expression:                (IEntityEventService entityEventService) =>
                    entityEventService.RaiseUpdateEventAsync(entityName:"updatedTeacher", entity:updatedTeacher)
            )
            .Returns(value:ValueTask.CompletedTask);

        TeacherOrchestrationService service = CreateTeacherOrchestrationService();

        ((object)(await service.UpdateTeacherAsync(updatedTeacher:updatedTeacher))).Should()
            .BeSameAs(expected:updatedTeacher, because:"");

        eventServiceMock.Verify(
expression:            (IEntityEventService eventService) => eventService.RaiseUpdateEventAsync(entityName:"updatedTeacher", entity:updatedTeacher),
times:            Times.Once
        );
    }
}