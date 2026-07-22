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
    public void GetSchoolsShouldReturnBrokerQuery()
    {
        // Given
        // When
        // Then
        IQueryable<School> expectedSchools = CreateSchools();

        schoolBrokerMock.Setup(expression:(ISchoolBroker broker) => broker.SelectAllSchools())
            .Returns(value:expectedSchools);

        SchoolService service = CreateSchoolService();
        IQueryable<School> actualSchools = service.GetSchools();

        ((IEnumerable<School>)actualSchools).Should()
            .BeSameAs(expected:expectedSchools, because:"");
    }
}