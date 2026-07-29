// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Teachers;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Teachers;

public sealed partial class TeacherServiceTests
{
    [Fact]
    public async Task AddTeacherAsyncShouldPersistAtomicCopy()
    {
        // Given
        // When
        // Then
        Teacher newTeacher = CreateTeacher();

        teacherBrokerMock
            .Setup(expression: (ITeacherBroker broker) => broker.InsertTeacherAsync(newTeacher: It.IsAny<Teacher>()))
            .Returns(valueFunction: () => ValueTask.FromResult(result: CreateTeacher()));

        TeacherService service = CreateTeacherService();
        Teacher actualTeacher = await service.AddTeacherAsync(newTeacher: newTeacher);

        ((object)actualTeacher).Should()
            .NotBeSameAs(unexpected: newTeacher, because: "");

        teacherBrokerMock.Verify(expression: (ITeacherBroker broker) => broker.InsertTeacherAsync(newTeacher: actualTeacher), times: Times.Once);
    }
}