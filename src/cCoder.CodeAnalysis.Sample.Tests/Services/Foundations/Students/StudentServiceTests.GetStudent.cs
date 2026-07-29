// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Exceptions;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Students;

public sealed partial class StudentServiceTests
{
    [Fact]
    public void GetStudentShouldReturnStudentOnHappyPath()
    {
        // Given
        // When
        // Then
        Student expectedStudent = new Student { Id = 7 };
        IQueryable<Student> students = new Student[1] { expectedStudent }.AsQueryable();

        studentBrokerMock.Setup(expression: (IStudentBroker broker) => broker.SelectAllStudents())
            .Returns(value: students);

        StudentService studentService = CreateStudentService();
        Student actualStudent = studentService.GetStudent(studentId: 7)!;

        ((object)actualStudent).Should()
            .BeSameAs(expected: expectedStudent, because: "");

        studentBrokerMock.Verify(expression: (IStudentBroker broker) => broker.SelectAllStudents(), times: Times.Once);
        studentBrokerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void GetStudentShouldWrapBrokerException()
    {
        // Given
        // When
        // Then
        InvalidOperationException brokerException = new InvalidOperationException();

        studentBrokerMock.Setup(expression: (IStudentBroker broker) => broker.SelectAllStudents())
            .Throws(exception: brokerException);

        StudentService studentService = CreateStudentService();

        Action getStudent = delegate
        {
            studentService.GetStudent(studentId: 7);
        };

        getStudent
            .Should()
            .Throw<StudentServiceDependencyException>(because: "", becauseArgs: [Array.Empty<object>()])
            .WithInnerException<InvalidOperationException>(because: "", becauseArgs: [Array.Empty<object>()]);
    }
}