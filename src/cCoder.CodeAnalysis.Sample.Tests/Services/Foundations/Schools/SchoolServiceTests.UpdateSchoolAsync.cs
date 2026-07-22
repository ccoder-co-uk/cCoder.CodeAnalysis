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
    public async Task UpdateSchoolAsyncShouldPersistAtomicCopy()
    {
        // Given
        // When
        // Then
        School updatedSchool = CreateSchool();

        schoolBrokerMock
            .Setup(expression:(ISchoolBroker broker) => broker.UpdateSchoolAsync(updatedSchool:It.IsAny<School>()))
            .Returns(valueFunction:() => ValueTask.FromResult(result:CreateSchool()));

        SchoolService service = CreateSchoolService();
        School actualSchool = await service.UpdateSchoolAsync(updatedSchool:updatedSchool);

        ((object)actualSchool).Should()
            .NotBeSameAs(unexpected:updatedSchool, because:"");

        schoolBrokerMock.Verify(expression:(ISchoolBroker broker) => broker.UpdateSchoolAsync(updatedSchool:actualSchool), times:Times.Once);
    }
}