// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Students;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.Students;

public sealed partial class StudentOrchestrationServiceTests
{
    [Fact]
    public async Task DeleteStudentAsyncShouldDeleteAndRaiseEventWhenStudentExists()
    {
        // Given
        // When
        // Then
        Student existingStudent = CreateStudent();

        studentServiceMock
            .Setup(expression: (IStudentService studentService) => studentService.GetStudent(studentId: 7))
            .Returns(value: existingStudent);

        studentServiceMock
            .Setup(expression: (IStudentService studentService) => studentService.DeleteStudentAsync(studentId: 7))
            .Returns(value: ValueTask.CompletedTask);

        eventServiceMock
            .Setup(
expression: (IEntityEventService entityEventService) =>
                    entityEventService.RaiseDeleteEventAsync(entityName: "updatedStudent", entity: existingStudent)
            )
            .Returns(value: ValueTask.CompletedTask);

        StudentOrchestrationService service = CreateStudentOrchestrationService();
        await service.DeleteStudentAsync(studentId: 7);
        studentServiceMock.Verify(expression: (IStudentService foundation) => foundation.DeleteStudentAsync(studentId: 7), times: Times.Once);
    }

    [Fact]
    public async Task DeleteStudentAsyncShouldStopWhenStudentDoesNotExist()
    {
        // Given
        // When
        // Then
        studentServiceMock
            .Setup(expression: (IStudentService studentService) => studentService.GetStudent(studentId: 7))
            .Returns(value: (Student?)null);

        StudentOrchestrationService service = CreateStudentOrchestrationService();
        await service.DeleteStudentAsync(studentId: 7);

        studentServiceMock.Verify(
expression: (IStudentService foundation) => foundation.DeleteStudentAsync(studentId: It.IsAny<int>()),
times: Times.Never
        );
    }
}