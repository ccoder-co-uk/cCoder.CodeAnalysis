// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Courses;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Courses;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.Courses;

public sealed partial class CourseOrchestrationServiceTests
{
    [Fact]
    public async Task DeleteCourseAsyncShouldDeleteAndRaiseEventWhenCourseExists()
    {
        // Given
        // When
        // Then
        Course existingCourse = new Course { Id = 7 };

        courseServiceMock.Setup(expression:(ICourseService courseService) => courseService.GetCourse(courseId:7))
            .Returns(value:existingCourse);

        courseServiceMock
            .Setup(expression:(ICourseService courseService) => courseService.DeleteCourseAsync(courseId:7))
            .Returns(value:ValueTask.CompletedTask);

        eventServiceMock
            .Setup(
expression:                (IEntityEventService entityEventService) =>
                    entityEventService.RaiseDeleteEventAsync(entityName:"updatedCourse", entity:existingCourse)
            )
            .Returns(value:ValueTask.CompletedTask);

        CourseOrchestrationService service = CreateCourseOrchestrationService();
        await service.DeleteCourseAsync(courseId:7);
        courseServiceMock.Verify(expression:(ICourseService foundation) => foundation.DeleteCourseAsync(courseId:7), times:Times.Once);
    }

    [Fact]
    public async Task DeleteCourseAsyncShouldStopWhenCourseDoesNotExist()
    {
        // Given
        // When
        // Then
        courseServiceMock.Setup(expression:(ICourseService courseService) => courseService.GetCourse(courseId:7))
            .Returns(value:(Course?)null);

        CourseOrchestrationService service = CreateCourseOrchestrationService();
        await service.DeleteCourseAsync(courseId:7);

        courseServiceMock.Verify(
expression:            (ICourseService foundation) => foundation.DeleteCourseAsync(courseId:It.IsAny<int>()),
times:            Times.Never
        );
    }
}