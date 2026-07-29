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
    public async Task AddOrUpdateCoursesAsyncShouldExerciseBothPersistenceBranches()
    {
        // Given
        // When
        // Then
        Course[] items = CreateCourses();

        courseServiceMock
            .Setup(expression: (ICourseService courseService) => courseService.AddCourseAsync(newCourse: items[0]))
            .Returns(valueFunction: () => ValueTask.FromResult(result: items[0]));

        courseServiceMock
            .Setup(expression: (ICourseService courseService) => courseService.UpdateCourseAsync(updatedCourse: items[1]))
            .Returns(valueFunction: () => ValueTask.FromResult(result: items[1]));

        CourseProcessingService service = CreateCourseProcessingService();
        await service.AddOrUpdateCoursesAsync(courses: items, schoolId: 11, teacherId: 13);
        courseServiceMock.Verify(expression: (ICourseService foundation) => foundation.AddCourseAsync(newCourse: items[0]), times: Times.Once);
        courseServiceMock.Verify(expression: (ICourseService foundation) => foundation.UpdateCourseAsync(updatedCourse: items[1]), times: Times.Once);
    }
}