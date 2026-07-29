// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Teachers;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Teachers;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.Teachers;

public sealed partial class TeacherOrchestrationServiceTests
{
    [Fact]
    public async Task DeleteTeacherAsyncShouldDeleteAndRaiseEventWhenTeacherExists()
    {
        // Given
        // When
        // Then
        Teacher existingTeacher = CreateTeacher();

        teacherServiceMock
            .Setup(expression: (ITeacherService teacherService) => teacherService.GetTeacher(teacherId: 7))
            .Returns(value: existingTeacher);

        teacherServiceMock
            .Setup(expression: (ITeacherService teacherService) => teacherService.DeleteTeacherAsync(teacherId: 7))
            .Returns(value: ValueTask.CompletedTask);

        eventServiceMock
            .Setup(
expression: (IEntityEventService entityEventService) =>
                    entityEventService.RaiseDeleteEventAsync(entityName: "updatedTeacher", entity: existingTeacher)
            )
            .Returns(value: ValueTask.CompletedTask);

        TeacherOrchestrationService service = CreateTeacherOrchestrationService();
        await service.DeleteTeacherAsync(teacherId: 7);
        teacherServiceMock.Verify(expression: (ITeacherService foundation) => foundation.DeleteTeacherAsync(teacherId: 7), times: Times.Once);
    }

    [Fact]
    public async Task DeleteTeacherAsyncShouldStopWhenTeacherDoesNotExist()
    {
        // Given
        // When
        // Then
        teacherServiceMock
            .Setup(expression: (ITeacherService teacherService) => teacherService.GetTeacher(teacherId: 7))
            .Returns(value: (Teacher?)null);

        TeacherOrchestrationService service = CreateTeacherOrchestrationService();
        await service.DeleteTeacherAsync(teacherId: 7);

        teacherServiceMock.Verify(
expression: (ITeacherService foundation) => foundation.DeleteTeacherAsync(teacherId: It.IsAny<int>()),
times: Times.Never
        );
    }
}