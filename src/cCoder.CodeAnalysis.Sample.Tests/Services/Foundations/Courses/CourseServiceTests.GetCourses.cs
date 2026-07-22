// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Courses;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Courses;

public sealed partial class CourseServiceTests
{
    [Fact]
    public void GetCoursesShouldReturnBrokerQuery()
    {
        // Given
        IQueryable<Course> expectedCourses = CreateCourses();

        courseBrokerMock
            .Setup(expression: (ICourseBroker broker) => broker.SelectAllCourses())
            .Returns(value: expectedCourses);

        CourseService service = CreateCourseService();

        // When
        IQueryable<Course> actualCourses = service.GetCourses();

        // Then

        ((IEnumerable<Course>)actualCourses)
            .Should()
            .BeSameAs(expected: expectedCourses, because: "");
    }
}