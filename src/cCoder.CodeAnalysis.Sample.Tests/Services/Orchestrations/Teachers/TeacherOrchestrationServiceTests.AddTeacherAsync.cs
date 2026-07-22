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
    public async Task AddTeacherAsyncShouldPersistAndRaiseEventOnHappyPath()
    {
        // Given
        // When
        // Then
        Teacher newTeacher = CreateTeacher();

        teacherServiceMock
            .Setup(expression:(ITeacherService teacherService) => teacherService.AddTeacherAsync(newTeacher:It.IsAny<Teacher>()))
            .Returns(valueFunction:() => ValueTask.FromResult(result:CreateTeacher()));

        eventServiceMock
            .Setup(
expression:                (IEntityEventService entityEventService) =>
                    entityEventService.RaiseAddEventAsync(entityName:"newTeacher", entity:newTeacher)
            )
            .Returns(value:ValueTask.CompletedTask);

        TeacherOrchestrationService service = CreateTeacherOrchestrationService();

        ((object)(await service.AddTeacherAsync(newTeacher:newTeacher))).Should()
            .BeSameAs(expected:newTeacher, because:"");

        eventServiceMock.Verify(
expression:            (IEntityEventService eventService) => eventService.RaiseAddEventAsync(entityName:"newTeacher", entity:newTeacher),
times:            Times.Once
        );
    }
}