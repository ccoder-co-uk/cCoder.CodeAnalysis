// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Students;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.Students;

public sealed partial class StudentOrchestrationServiceTests
{
    [Fact]
    public async Task AddStudentAsyncShouldPersistAndRaiseEventOnHappyPath()
    {
        // Given
        // When
        // Then
        Student newStudent = CreateStudent();

        studentServiceMock
            .Setup(expression: (IStudentService studentService) => studentService.AddStudentAsync(newStudent: It.IsAny<Student>()))
            .Returns(valueFunction: () => ValueTask.FromResult(result: CreateStudent()));

        eventServiceMock
            .Setup(
expression: (IEntityEventService entityEventService) =>
                    entityEventService.RaiseAddEventAsync(entityName: "newStudent", entity: newStudent)
            )
            .Returns(value: ValueTask.CompletedTask);

        StudentOrchestrationService service = CreateStudentOrchestrationService();

        ((object)(await service.AddStudentAsync(newStudent: newStudent))).Should()
            .BeSameAs(expected: newStudent, because: "");

        eventServiceMock.Verify(
expression: (IEntityEventService eventService) => eventService.RaiseAddEventAsync(entityName: "newStudent", entity: newStudent),
times: Times.Once
        );
    }
}