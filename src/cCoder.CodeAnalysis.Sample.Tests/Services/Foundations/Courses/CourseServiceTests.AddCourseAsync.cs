// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Courses;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Courses;

public sealed partial class CourseServiceTests
{
    [Fact]
    public async Task AddCourseAsyncShouldPersistAtomicCopy()
    {
        // Given
        Course newCourse = CreateCourse();

        courseBrokerMock
            .Setup(expression: (ICourseBroker broker) => broker.InsertCourseAsync(
                newCourse: It.IsAny<Course>()))
            .Returns(valueFunction: () => ValueTask.FromResult(
                result: CreateCourse()));

        CourseService service = CreateCourseService();

        // When

        Course actualCourse = await service.AddCourseAsync(
            newCourse: newCourse);

        // Then

        ((object)actualCourse)
            .Should()
            .NotBeSameAs(unexpected: newCourse, because: "");

        courseBrokerMock.Verify(
            expression: (ICourseBroker broker) => broker.InsertCourseAsync(
                newCourse: actualCourse),
            times: Times.Once);
    }
}