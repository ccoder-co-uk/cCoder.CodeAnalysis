// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Students;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.Students;

public sealed partial class StudentOrchestrationServiceTests
{
    [Fact]
    public void GetStudentShouldReturnStudentOnHappyPath()
    {
        // Given
        // When
        // Then
        Student expectedStudent = CreateStudent();

        studentServiceMock
            .Setup(expression:(IStudentService studentService) => studentService.GetStudent(studentId:7))
            .Returns(value:expectedStudent);

        StudentOrchestrationService service = CreateStudentOrchestrationService();
        Student actualStudent = service.GetStudent(studentId:7)!;

        ((object)actualStudent).Should()
            .BeSameAs(expected:expectedStudent, because:"");
    }
}