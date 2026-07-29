// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Teachers;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Teachers;

public sealed partial class TeacherServiceTests
{
    [Fact]
    public async Task DeleteTeacherAsyncShouldDeleteExistingTeacher()
    {
        // Given
        // When
        // Then
        Teacher existingTeacher = CreateTeacher();

        teacherBrokerMock
            .Setup(expression: (ITeacherBroker broker) => broker.SelectAllTeachers())
            .Returns(value: CreateTeachers(teacher: existingTeacher));

        teacherBrokerMock
            .Setup(expression: (ITeacherBroker broker) => broker.DeleteTeacherAsync(deletedTeacher: existingTeacher))
            .Returns(valueFunction: () => ValueTask.FromResult(result: 1));

        TeacherService service = CreateTeacherService();
        await service.DeleteTeacherAsync(teacherId: existingTeacher.Id);
        teacherBrokerMock.Verify(expression: (ITeacherBroker broker) => broker.DeleteTeacherAsync(deletedTeacher: existingTeacher), times: Times.Once);
    }

    [Fact]
    public async Task DeleteTeacherAsyncShouldNotDeleteMissingTeacher()
    {
        // Given
        // When
        // Then
        teacherBrokerMock
            .Setup(expression: (ITeacherBroker broker) => broker.SelectAllTeachers())
            .Returns(value: Array.Empty<Teacher>()
                .AsQueryable());

        TeacherService service = CreateTeacherService();
        await service.DeleteTeacherAsync(teacherId: 7);

        teacherBrokerMock.Verify(
expression: (ITeacherBroker broker) => broker.DeleteTeacherAsync(deletedTeacher: It.IsAny<Teacher>()),
times: Times.Never
        );
    }
}