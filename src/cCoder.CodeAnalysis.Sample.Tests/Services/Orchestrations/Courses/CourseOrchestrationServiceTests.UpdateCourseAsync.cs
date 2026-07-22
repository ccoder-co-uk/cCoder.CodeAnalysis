// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Courses;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Courses;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.Courses;

public sealed partial class CourseOrchestrationServiceTests
{
    [Fact]
    public async Task UpdateCourseAsyncShouldPersistAndRaiseEventOnHappyPath()
    {
        // Given
        // When
        // Then
        Course updatedCourse = new Course { Id = 7 };

        courseServiceMock
            .Setup(expression:(ICourseService courseService) => courseService.UpdateCourseAsync(updatedCourse:It.IsAny<Course>()))
            .Returns(valueFunction:() => ValueTask.FromResult(result:new Course { Id = 7 }));

        eventServiceMock
            .Setup(
expression:                (IEntityEventService entityEventService) =>
                    entityEventService.RaiseUpdateEventAsync(entityName:"updatedCourse", entity:updatedCourse)
            )
            .Returns(value:ValueTask.CompletedTask);

        CourseOrchestrationService service = CreateCourseOrchestrationService();

        ((object)(await service.UpdateCourseAsync(updatedCourse:updatedCourse))).Should()
            .BeSameAs(expected:updatedCourse, because:"");

        eventServiceMock.Verify(
expression:            (IEntityEventService eventService) => eventService.RaiseUpdateEventAsync(entityName:"updatedCourse", entity:updatedCourse),
times:            Times.Once
        );
    }
}