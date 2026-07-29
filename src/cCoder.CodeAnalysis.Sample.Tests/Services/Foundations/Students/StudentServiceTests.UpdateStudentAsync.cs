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
    public async Task UpdateStudentAsyncShouldPersistAtomicCopyOnHappyPath()
    {
        // Given
        // When
        // Then
        Student updatedStudent = new Student
        {
            Id = 7,
            Courses = new List<Course>(1) { new Course() },
        };

        Student? persistedStudent = null;

        studentBrokerMock
            .Setup(expression: (IStudentBroker broker) => broker.UpdateStudentAsync(updatedStudent: It.IsAny<Student>()))
            .Callback(
action: delegate (Student student)
                {
                    persistedStudent = student;
                }
            )
            .Returns(valueFunction: () => ValueTask.FromResult(result: new Student()));

        StudentService studentService = CreateStudentService();
        Student actualStudent = await studentService.UpdateStudentAsync(updatedStudent: updatedStudent);

        ((object)actualStudent).Should()
            .BeSameAs(expected: persistedStudent, because: "").And.NotBeSameAs(unexpected: updatedStudent, because: "");

        ((IEnumerable<Course>)actualStudent.Courses).Should()
            .BeEmpty(because: "");
    }

    [Fact]
    public async Task UpdateStudentAsyncShouldThrowValidationExceptionForNullStudent()
    {
        // Given
        // When
        // Then
        StudentService studentService = CreateStudentService();

        Func<Task> updateStudent = async delegate
        {
            await studentService.UpdateStudentAsync(updatedStudent: null!);
        };

        await updateStudent.Should()
            .ThrowAsync<StudentServiceValidationException>(because: "", becauseArgs: [Array.Empty<object>()]);
    }

    [Fact]
    public async Task UpdateStudentAsyncShouldWrapBrokerException()
    {
        // Given
        // When
        // Then
        studentBrokerMock
            .Setup(expression: (IStudentBroker broker) => broker.UpdateStudentAsync(updatedStudent: It.IsAny<Student>()))
            .Throws(exception: new InvalidOperationException());

        StudentService studentService = CreateStudentService();

        Func<Task> updateStudent = async delegate
        {
            await studentService.UpdateStudentAsync(updatedStudent: new Student());
        };

        await updateStudent.Should()
            .ThrowAsync<StudentServiceDependencyException>(because: "", becauseArgs: [Array.Empty<object>()]);
    }
}