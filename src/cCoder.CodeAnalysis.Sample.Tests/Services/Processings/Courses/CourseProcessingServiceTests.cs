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
    private readonly Mock<ICourseService> courseServiceMock = new Mock<ICourseService>();

    private CourseProcessingService CreateCourseProcessingService()
    {
        return new CourseProcessingService(courseServiceMock.Object);
    }

    private static Course CreateCourse(int courseId)
    {
        return new Course { Id = courseId };
    }

    private static Course[] CreateCourses()
    {
        return new Course[2] { CreateCourse(courseId:0), CreateCourse(courseId:7) };
    }
}