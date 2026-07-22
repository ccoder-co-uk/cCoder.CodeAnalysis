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
    public async Task AddCourseAsyncShouldPersistAndRaiseEventOnHappyPath()
    {
        // Given
        // When
        // Then
        Course newCourse = new Course { Id = 7 };

        courseServiceMock
            .Setup(expression:(ICourseService courseService) => courseService.AddCourseAsync(newCourse:It.IsAny<Course>()))
            .Returns(valueFunction:() => ValueTask.FromResult(result:new Course { Id = 7 }));

        eventServiceMock
            .Setup(
expression:                (IEntityEventService entityEventService) =>
                    entityEventService.RaiseAddEventAsync(entityName:"newCourse", entity:newCourse)
            )
            .Returns(value:ValueTask.CompletedTask);

        CourseOrchestrationService service = CreateCourseOrchestrationService();

        ((object)(await service.AddCourseAsync(newCourse:newCourse))).Should()
            .BeSameAs(expected:newCourse, because:"");

        eventServiceMock.Verify(
expression:            (IEntityEventService eventService) => eventService.RaiseAddEventAsync(entityName:"newCourse", entity:newCourse),
times:            Times.Once
        );
    }
}