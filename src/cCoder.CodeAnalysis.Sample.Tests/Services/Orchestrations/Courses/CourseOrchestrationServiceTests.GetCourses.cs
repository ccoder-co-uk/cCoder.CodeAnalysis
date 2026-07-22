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
    public void GetCoursesShouldReturnCoursesOnHappyPath()
    {
        // Given
        // When
        // Then
        IQueryable<Course> expectedCourses = new Course[1] { new Course { Id = 7 } }.AsQueryable();

        courseServiceMock.Setup(expression:(ICourseService courseService) => courseService.GetCourses())
            .Returns(value:expectedCourses);

        CourseOrchestrationService service = CreateCourseOrchestrationService();
        IQueryable<Course> actualCourses = service.GetCourses();

        ((IEnumerable<Course>)actualCourses).Should()
            .BeSameAs(expected:expectedCourses, because:"");
    }
}