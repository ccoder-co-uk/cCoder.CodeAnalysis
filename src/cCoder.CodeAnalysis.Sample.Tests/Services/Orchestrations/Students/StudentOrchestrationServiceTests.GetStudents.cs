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
    public void GetStudentsShouldReturnStudentsOnHappyPath()
    {
        // Given
        // When
        // Then
        IQueryable<Student> expectedStudents = CreateStudents(student: CreateStudent());

        studentServiceMock
            .Setup(expression: (IStudentService studentService) => studentService.GetStudents())
            .Returns(value: expectedStudents);

        StudentOrchestrationService service = CreateStudentOrchestrationService();
        IQueryable<Student> actualStudents = service.GetStudents();

        ((IEnumerable<Student>)actualStudents).Should()
            .BeSameAs(expected: expectedStudents, because: "");
    }
}