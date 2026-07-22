// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Courses;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Courses;

public sealed partial class CourseServiceTests
{
    [Fact]
    public async Task DeleteCourseAsyncShouldDeleteExistingCourse()
    {
        // Given
        Course existingCourse = CreateCourse();

        courseBrokerMock
            .Setup(expression: (ICourseBroker broker) => broker.SelectAllCourses())
            .Returns(value: CreateCourses(course: existingCourse));

        courseBrokerMock
            .Setup(expression: (ICourseBroker broker) => broker.DeleteCourseAsync(
                deletedCourse: existingCourse))
            .Returns(valueFunction: () => ValueTask.FromResult(
                result: 1));

        CourseService service = CreateCourseService();

        // When

        await service.DeleteCourseAsync(
            courseId: existingCourse.Id);

        // Then

        courseBrokerMock.Verify(
            expression: (ICourseBroker broker) => broker.DeleteCourseAsync(
                deletedCourse: existingCourse),
            times: Times.Once);
    }

    [Fact]
    public async Task DeleteCourseAsyncShouldNotDeleteMissingCourse()
    {
        // Given
        courseBrokerMock
            .Setup(expression: (ICourseBroker broker) => broker.SelectAllCourses())
            .Returns(value: Array.Empty<Course>()
                .AsQueryable());

        CourseService service = CreateCourseService();

        // When

        await service.DeleteCourseAsync(
            courseId: 7);

        // Then

        courseBrokerMock.Verify(
            expression: (ICourseBroker broker) => broker.DeleteCourseAsync(
                deletedCourse: It.IsAny<Course>()),
            times: Times.Never);
    }
}