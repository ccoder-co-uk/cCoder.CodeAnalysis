// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Courses;
using cCoder.CodeAnalysis.Sample.Services.Processings.Courses;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Processings.Courses;

public sealed partial class CourseProcessingServiceTests
{
    [Fact]
    public async Task DeleteCoursesAsyncShouldDeleteEveryItem()
    {
        // Given
        // When
        // Then
        Course[] items = CreateCourses();

        courseServiceMock
            .Setup(expression:(ICourseService courseService) => courseService.DeleteCourseAsync(courseId:It.IsAny<int>()))
            .Returns(value:ValueTask.CompletedTask);

        CourseProcessingService service = CreateCourseProcessingService();
        await service.DeleteCoursesAsync(deletedCourses:items);

        courseServiceMock.Verify(
expression:            (ICourseService foundation) => foundation.DeleteCourseAsync(courseId:It.IsAny<int>()),
times:            Times.Exactly(callCount:2)
        );
    }
}