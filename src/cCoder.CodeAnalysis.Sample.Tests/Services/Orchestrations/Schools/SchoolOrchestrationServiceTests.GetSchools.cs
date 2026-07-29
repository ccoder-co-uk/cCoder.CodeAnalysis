// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Schools;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.Schools;

public sealed partial class SchoolOrchestrationServiceTests
{
    [Fact]
    public void GetSchoolsShouldReturnSchoolsOnHappyPath()
    {
        // Given
        // When
        // Then
        IQueryable<School> expectedSchools = CreateSchools(school: CreateSchool());

        schoolServiceMock.Setup(expression: (ISchoolService schoolService) => schoolService.GetSchools())
            .Returns(value: expectedSchools);

        SchoolOrchestrationService service = CreateSchoolOrchestrationService();
        IQueryable<School> actualSchools = service.GetSchools();

        ((IEnumerable<School>)actualSchools).Should()
            .BeSameAs(expected: expectedSchools, because: "");
    }
}