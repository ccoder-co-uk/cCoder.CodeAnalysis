// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Schools;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Schools;

public sealed partial class SchoolServiceTests
{
    [Fact]
    public void GetSchoolShouldReturnMatchingSchool()
    {
        // Given
        // When
        // Then
        School expectedSchool = CreateSchool();

        schoolBrokerMock
            .Setup(expression: (ISchoolBroker broker) => broker.SelectAllSchools())
            .Returns(value: CreateSchools(school: expectedSchool));

        SchoolService service = CreateSchoolService();
        School actualSchool = service.GetSchool(schoolId: expectedSchool.Id)!;

        ((object)actualSchool).Should()
            .BeSameAs(expected: expectedSchool, because: "");
    }
}