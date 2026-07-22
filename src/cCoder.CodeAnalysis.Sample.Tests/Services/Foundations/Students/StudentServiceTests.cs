// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Students;

public sealed partial class StudentServiceTests
{
    private readonly Mock<IStudentBroker> studentBrokerMock = new Mock<IStudentBroker>();

    private StudentService CreateStudentService()
    {
        return new StudentService(studentBrokerMock.Object);
    }

    private static Student CreateStudent(int studentId = 7)
    {
        return new Student { Id = studentId };
    }

    private static IQueryable<Student> CreateStudents(Student? student = null)
    {
        return new Student[1] { student ?? CreateStudent() }.AsQueryable();
    }

    private static Course CreateCourse()
    {
        return new Course();
    }

    private static InvalidOperationException CreateInvalidOperationException()
    {
        return new InvalidOperationException();
    }
}