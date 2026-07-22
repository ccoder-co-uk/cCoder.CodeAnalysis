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
    public async Task DeleteStudentAsyncShouldDeleteExistingStudentOnHappyPath()
    {
        // Given
        // When
        // Then
        Student student = new Student { Id = 7 };

        studentBrokerMock
            .Setup(expression:(IStudentBroker broker) => broker.SelectAllStudents())
            .Returns(value:new Student[1] { student }.AsQueryable());

        studentBrokerMock
            .Setup(expression:(IStudentBroker broker) => broker.DeleteStudentAsync(deletedStudent:student))
            .Returns(valueFunction:() => ValueTask.FromResult(result:1));

        StudentService studentService = CreateStudentService();
        await studentService.DeleteStudentAsync(studentId:student.Id);
        studentBrokerMock.Verify(expression:(IStudentBroker broker) => broker.DeleteStudentAsync(deletedStudent:student), times:Times.Once);
    }

    [Fact]
    public async Task DeleteStudentAsyncShouldNotDeleteMissingStudent()
    {
        // Given
        // When
        // Then
        studentBrokerMock
            .Setup(expression:(IStudentBroker broker) => broker.SelectAllStudents())
            .Returns(value:Array.Empty<Student>()
                .AsQueryable());

        StudentService studentService = CreateStudentService();
        await studentService.DeleteStudentAsync(studentId:7);

        studentBrokerMock.Verify(
expression:            (IStudentBroker broker) => broker.DeleteStudentAsync(deletedStudent:It.IsAny<Student>()),
times:            Times.Never
        );
    }

    [Fact]
    public async Task DeleteStudentAsyncShouldWrapBrokerException()
    {
        // Given
        // When
        // Then
        studentBrokerMock
            .Setup(expression:(IStudentBroker broker) => broker.SelectAllStudents())
            .Throws(exception:new InvalidOperationException());

        StudentService studentService = CreateStudentService();

        Func<Task> deleteStudent = async delegate
        {
            await studentService.DeleteStudentAsync(studentId:7);
        };

        await deleteStudent.Should()
            .ThrowAsync<StudentServiceDependencyException>(because:"",becauseArgs:[Array.Empty<object>()]);
    }
}