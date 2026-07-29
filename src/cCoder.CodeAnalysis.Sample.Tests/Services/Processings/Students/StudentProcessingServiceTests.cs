// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;
using cCoder.CodeAnalysis.Sample.Services.Processings.Students;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Processings.Students;

public sealed partial class StudentProcessingServiceTests
{
    private readonly Mock<IStudentService> studentServiceMock = new Mock<IStudentService>();

    private StudentProcessingService CreateStudentProcessingService()
    {
        return new StudentProcessingService(studentServiceMock.Object);
    }

    private static Student CreateStudent(int studentId)
    {
        return new Student { Id = studentId };
    }

    private static Student[] CreateStudents()
    {
        return new Student[2] { CreateStudent(studentId: 0), CreateStudent(studentId: 7) };
    }
}