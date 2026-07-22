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
    private readonly Mock<ICourseService> courseServiceMock = new Mock<ICourseService>();

    private readonly Mock<IEntityEventService> eventServiceMock = new Mock<IEntityEventService>();

    private CourseOrchestrationService CreateCourseOrchestrationService()
    {
        return new CourseOrchestrationService(courseServiceMock.Object, eventServiceMock.Object);
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