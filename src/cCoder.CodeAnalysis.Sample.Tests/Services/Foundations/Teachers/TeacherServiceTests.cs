// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Teachers;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Teachers;

public sealed partial class TeacherServiceTests
{
    private readonly Mock<ITeacherBroker> teacherBrokerMock = new Mock<ITeacherBroker>();

    private TeacherService CreateTeacherService()
    {
        return new TeacherService(teacherBrokerMock.Object);
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