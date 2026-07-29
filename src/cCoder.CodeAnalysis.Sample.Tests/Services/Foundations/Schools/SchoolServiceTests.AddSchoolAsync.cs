// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Schools;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Schools;

public sealed partial class SchoolServiceTests
{
    [Fact]
    public async Task AddSchoolAsyncShouldPersistAtomicCopy()
    {
        // Given
        // When
        // Then
        School newSchool = CreateSchool();

        schoolBrokerMock
            .Setup(expression: (ISchoolBroker broker) => broker.InsertSchoolAsync(newSchool: It.IsAny<School>()))
            .Returns(valueFunction: () => ValueTask.FromResult(result: CreateSchool()));

        SchoolService service = CreateSchoolService();
        School actualSchool = await service.AddSchoolAsync(newSchool: newSchool);

        ((object)actualSchool).Should()
            .NotBeSameAs(unexpected: newSchool, because: "");

        schoolBrokerMock.Verify(expression: (ISchoolBroker broker) => broker.InsertSchoolAsync(newSchool: actualSchool), times: Times.Once);
    }
}