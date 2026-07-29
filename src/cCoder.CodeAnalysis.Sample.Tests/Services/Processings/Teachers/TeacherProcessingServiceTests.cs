// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Teachers;
using cCoder.CodeAnalysis.Sample.Services.Processings.Teachers;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Processings.Teachers;

public sealed partial class TeacherProcessingServiceTests
{
    private readonly Mock<ITeacherService> teacherServiceMock = new Mock<ITeacherService>();

    private TeacherProcessingService CreateTeacherProcessingService()
    {
        return new TeacherProcessingService(teacherServiceMock.Object);
    }

    private static Teacher CreateTeacher(int teacherId)
    {
        return new Teacher { Id = teacherId };
    }

    private static Teacher[] CreateTeachers()
    {
        return new Teacher[2] { CreateTeacher(teacherId: 0), CreateTeacher(teacherId: 7) };
    }
}