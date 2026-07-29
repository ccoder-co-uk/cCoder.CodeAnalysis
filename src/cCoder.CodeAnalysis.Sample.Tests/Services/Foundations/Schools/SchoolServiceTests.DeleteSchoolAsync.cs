// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Schools;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Schools;

public sealed partial class SchoolServiceTests
{
    [Fact]
    public async Task DeleteSchoolAsyncShouldDeleteExistingSchool()
    {
        // Given
        // When
        // Then
        School existingSchool = CreateSchool();

        schoolBrokerMock
            .Setup(expression: (ISchoolBroker broker) => broker.SelectAllSchools())
            .Returns(value: CreateSchools(school: existingSchool));

        schoolBrokerMock
            .Setup(expression: (ISchoolBroker broker) => broker.DeleteSchoolAsync(deletedSchool: existingSchool))
            .Returns(valueFunction: () => ValueTask.FromResult(result: 1));

        SchoolService service = CreateSchoolService();
        await service.DeleteSchoolAsync(schoolId: existingSchool.Id);
        schoolBrokerMock.Verify(expression: (ISchoolBroker broker) => broker.DeleteSchoolAsync(deletedSchool: existingSchool), times: Times.Once);
    }

    [Fact]
    public async Task DeleteSchoolAsyncShouldNotDeleteMissingSchool()
    {
        // Given
        // When
        // Then
        schoolBrokerMock
            .Setup(expression: (ISchoolBroker broker) => broker.SelectAllSchools())
            .Returns(value: Array.Empty<School>()
                .AsQueryable());

        SchoolService service = CreateSchoolService();
        await service.DeleteSchoolAsync(schoolId: 7);
        schoolBrokerMock.Verify(expression: (ISchoolBroker broker) => broker.DeleteSchoolAsync(deletedSchool: It.IsAny<School>()), times: Times.Never);
    }
}