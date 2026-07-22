// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Runtime.InteropServices;
using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Tests.Sample.Services.Foundations.Students;

public sealed class StudentServiceTests
{
    [Fact]
    public async Task AddStudentAsyncReturnsThePersistedFlatCopy()
    {
        Mock<IStudentBroker> brokerMock = new Mock<IStudentBroker>();
        Student? persistedStudent = null;
        brokerMock
            .Setup((IStudentBroker broker) => broker.InsertStudentAsync(It.IsAny<Student>()))
            .Callback(
                delegate(Student student)
                {
                    persistedStudent = student;
                    student.Id = 42;
                }
            )
            .ReturnsAsync(new Student { Id = 999 });
        StudentService service = new StudentService(brokerMock.Object);
        Student newStudent = CreateStudent();
        Student result = await service.AddStudentAsync(newStudent);
        ((object)result).Should().BeSameAs(persistedStudent, "");
        ((object)result).Should().NotBeSameAs(newStudent, "");
        result.Id.Should().Be(42, "");
        ((object)result.School).Should().BeNull("");
        ((IEnumerable<Course>)result.Courses).Should().BeEmpty("");
    }

    [Fact]
    public async Task UpdateStudentAsyncReturnsThePersistedFlatCopy()
    {
        Mock<IStudentBroker> brokerMock = new Mock<IStudentBroker>();
        Student? persistedStudent = null;
        brokerMock
            .Setup((IStudentBroker broker) => broker.UpdateStudentAsync(It.IsAny<Student>()))
            .Callback(
                delegate(Student student)
                {
                    persistedStudent = student;
                    student.LastName = "Persisted";
                }
            )
            .ReturnsAsync(new Student { Id = 999 });
        StudentService service = new StudentService(brokerMock.Object);
        Student updatedStudent = CreateStudent();
        Student result = await service.UpdateStudentAsync(updatedStudent);
        ((object)result).Should().BeSameAs(persistedStudent, "");
        ((object)result).Should().NotBeSameAs(updatedStudent, "");
        result.LastName.Should().Be("Persisted", "");
        ((object)result.School).Should().BeNull("");
        ((IEnumerable<Course>)result.Courses).Should().BeEmpty("");
    }

    private static Student CreateStudent()
    {
        Student obj = new Student
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            SchoolId = 1,
            School = new School { Id = 1, Name = "Sample School" },
        };
        int num = 1;
        List<Course> list = new List<Course>(num);
        CollectionsMarshal.SetCount(list, num);
        CollectionsMarshal.AsSpan(list)[0] = new Course { Id = 1, Name = "Computing" };
        obj.Courses = list;
        return obj;
    }
}