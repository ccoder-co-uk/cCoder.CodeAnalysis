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
    public async Task AddStudentAsyncShouldPersistAtomicCopyOnHappyPath()
    {
        // Given
        // When
        // Then
        Student newStudent = new Student
        {
            Id = 7,
            FirstName = "Ada",
            Courses = new List<Course>(1) { new Course() },
        };

        Student? persistedStudent = null;

        studentBrokerMock
            .Setup(expression: (IStudentBroker broker) => broker.InsertStudentAsync(newStudent: It.IsAny<Student>()))
            .Callback(
action: delegate (Student student)
                {
                    persistedStudent = student;
                }
            )
            .Returns(valueFunction: () => ValueTask.FromResult(result: new Student()));

        StudentService studentService = CreateStudentService();
        Student actualStudent = await studentService.AddStudentAsync(newStudent: newStudent);

        ((object)actualStudent).Should()
            .BeSameAs(expected: persistedStudent, because: "").And.NotBeSameAs(unexpected: newStudent, because: "");

        ((IEnumerable<Course>)actualStudent.Courses).Should()
            .BeEmpty(because: "");
    }

    [Fact]
    public async Task AddStudentAsyncShouldThrowValidationExceptionForNullStudent()
    {
        // Given
        // When
        // Then
        StudentService studentService = CreateStudentService();

        Func<Task> addStudent = async delegate
        {
            await studentService.AddStudentAsync(newStudent: null!);
        };

        await addStudent.Should()
            .ThrowAsync<StudentServiceValidationException>(because: "", becauseArgs: [Array.Empty<object>()]);
    }

    [Fact]
    public async Task AddStudentAsyncShouldWrapBrokerException()
    {
        // Given
        // When
        // Then
        studentBrokerMock
            .Setup(expression: (IStudentBroker broker) => broker.InsertStudentAsync(newStudent: It.IsAny<Student>()))
            .Throws(exception: new InvalidOperationException());

        StudentService studentService = CreateStudentService();

        Func<Task> addStudent = async delegate
        {
            await studentService.AddStudentAsync(newStudent: new Student());
        };

        await addStudent.Should()
            .ThrowAsync<StudentServiceDependencyException>(because: "", becauseArgs: [Array.Empty<object>()]);
    }
}