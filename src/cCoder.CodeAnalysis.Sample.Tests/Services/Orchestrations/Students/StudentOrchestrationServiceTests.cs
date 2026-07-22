// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Students;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.Students;

public sealed partial class StudentOrchestrationServiceTests
{
    private readonly Mock<IStudentService> studentServiceMock = new Mock<IStudentService>();

    private readonly Mock<IEntityEventService> eventServiceMock = new Mock<IEntityEventService>();

    private StudentOrchestrationService CreateStudentOrchestrationService()
    {
        return new StudentOrchestrationService(studentServiceMock.Object, eventServiceMock.Object);
    }

    private static Student CreateStudent(int studentId = 7)
    {
        return new Student { Id = studentId };
    }

    private static IQueryable<Student> CreateStudents(Student? student = null)
    {
        return new Student[1] { student ?? CreateStudent() }.AsQueryable();
    }
}