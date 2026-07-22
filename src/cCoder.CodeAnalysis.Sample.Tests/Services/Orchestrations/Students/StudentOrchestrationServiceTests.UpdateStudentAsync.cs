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
    public async Task UpdateStudentAsyncShouldPersistAndRaiseEventOnHappyPath()
    {
        // Given
        // When
        // Then
        Student updatedStudent = CreateStudent();

        studentServiceMock
            .Setup(expression:(IStudentService studentService) => studentService.UpdateStudentAsync(updatedStudent:It.IsAny<Student>()))
            .Returns(valueFunction:() => ValueTask.FromResult(result:CreateStudent()));

        eventServiceMock
            .Setup(
expression:                (IEntityEventService entityEventService) =>
                    entityEventService.RaiseUpdateEventAsync(entityName:"updatedStudent", entity:updatedStudent)
            )
            .Returns(value:ValueTask.CompletedTask);

        StudentOrchestrationService service = CreateStudentOrchestrationService();

        ((object)(await service.UpdateStudentAsync(updatedStudent:updatedStudent))).Should()
            .BeSameAs(expected:updatedStudent, because:"");

        eventServiceMock.Verify(
expression:            (IEntityEventService eventService) => eventService.RaiseUpdateEventAsync(entityName:"updatedStudent", entity:updatedStudent),
times:            Times.Once
        );
    }
}