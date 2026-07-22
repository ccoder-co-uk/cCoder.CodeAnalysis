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
    public void GetCourseShouldReturnMatchingCourse()
    {
        // Given
        Course expectedCourse = CreateCourse();

        courseBrokerMock
            .Setup(expression: (ICourseBroker broker) => broker.SelectAllCourses())
            .Returns(value: CreateCourses(course: expectedCourse));

        CourseService service = CreateCourseService();

        // When

        Course actualCourse = service.GetCourse(
            courseId: expectedCourse.Id)!;

        // Then

        ((object)actualCourse)
            .Should()
            .BeSameAs(expected: expectedCourse, because: "");
    }
}