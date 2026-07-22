// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Teachers;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Teachers;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.Teachers;

public sealed partial class TeacherOrchestrationServiceTests
{
    private readonly Mock<ITeacherService> teacherServiceMock = new Mock<ITeacherService>();

    private readonly Mock<IEntityEventService> eventServiceMock = new Mock<IEntityEventService>();

    private TeacherOrchestrationService CreateTeacherOrchestrationService()
    {
        return new TeacherOrchestrationService(teacherServiceMock.Object, eventServiceMock.Object);
    }

    private static Teacher CreateTeacher(int teacherId = 7)
    {
        return new Teacher { Id = teacherId };
    }

    private static IQueryable<Teacher> CreateTeachers(Teacher? teacher = null)
    {
        return new Teacher[1] { teacher ?? CreateTeacher() }.AsQueryable();
    }
}