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
    private readonly Mock<ICourseBroker> courseBrokerMock = new Mock<ICourseBroker>();

    private CourseService CreateCourseService()
    {
        return new CourseService(courseBrokerMock.Object);
    }

    private static Course CreateCourse(int courseId = 7)
    {
        return new Course { Id = courseId };
    }

    private static IQueryable<Course> CreateCourses(Course? course = null)
    {
        return new Course[1] { course ?? CreateCourse() }.AsQueryable();
    }
}