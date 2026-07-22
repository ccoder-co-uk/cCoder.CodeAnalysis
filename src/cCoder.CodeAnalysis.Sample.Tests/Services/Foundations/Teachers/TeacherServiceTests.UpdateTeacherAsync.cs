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
    public async Task UpdateTeacherAsyncShouldPersistAtomicCopy()
    {
        // Given
        // When
        // Then
        Teacher updatedTeacher = CreateTeacher();

        teacherBrokerMock
            .Setup(expression:(ITeacherBroker broker) => broker.UpdateTeacherAsync(updatedTeacher:It.IsAny<Teacher>()))
            .Returns(valueFunction:() => ValueTask.FromResult(result:CreateTeacher()));

        TeacherService service = CreateTeacherService();
        Teacher actualTeacher = await service.UpdateTeacherAsync(updatedTeacher:updatedTeacher);

        ((object)actualTeacher).Should()
            .NotBeSameAs(unexpected:updatedTeacher, because:"");

        teacherBrokerMock.Verify(expression:(ITeacherBroker broker) => broker.UpdateTeacherAsync(updatedTeacher:actualTeacher), times:Times.Once);
    }
}