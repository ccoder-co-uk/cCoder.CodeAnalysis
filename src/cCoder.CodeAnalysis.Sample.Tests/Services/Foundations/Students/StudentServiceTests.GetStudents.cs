// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Exceptions;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Students;

public sealed partial class StudentServiceTests
{
    [Fact]
    public void GetStudentsShouldReturnStudentsOnHappyPath()
    {
        // Given
        // When
        // Then
        IQueryable<Student> expectedStudents = new Student[1] { new Student { Id = 7 } }.AsQueryable();

        studentBrokerMock.Setup(expression:(IStudentBroker broker) => broker.SelectAllStudents())
            .Returns(value:expectedStudents);

        StudentService studentService = CreateStudentService();
        IQueryable<Student> actualStudents = studentService.GetStudents();

        ((IEnumerable<Student>)actualStudents).Should()
            .BeSameAs(expected:expectedStudents, because:"");
    }

    [Fact]
    public void GetStudentsShouldWrapBrokerException()
    {
        // Given
        // When
        // Then
        studentBrokerMock
            .Setup(expression:(IStudentBroker broker) => broker.SelectAllStudents())
            .Throws(exception:new InvalidOperationException());

        StudentService studentService = CreateStudentService();

        Action getStudents = delegate
        {
            studentService.GetStudents();
        };

        getStudents.Should()
            .Throw<StudentServiceDependencyException>(because:"",becauseArgs:[Array.Empty<object>()]);
    }
}