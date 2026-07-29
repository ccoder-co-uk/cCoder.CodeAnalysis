// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Courses;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Courses;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.Courses;

public sealed partial class CourseOrchestrationServiceTests
{
    [Fact]
    public void GetCourseShouldReturnCourseOnHappyPath()
    {
        // Given
        // When
        // Then
        Course expectedCourse = new Course { Id = 7 };

        courseServiceMock.Setup(expression: (ICourseService courseService) => courseService.GetCourse(courseId: 7))
            .Returns(value: expectedCourse);

        CourseOrchestrationService service = CreateCourseOrchestrationService();
        Course actualCourse = service.GetCourse(courseId: 7)!;

        ((object)actualCourse).Should()
            .BeSameAs(expected: expectedCourse, because: "");
    }
}