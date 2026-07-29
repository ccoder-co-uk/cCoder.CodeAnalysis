// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Coordinations.SchoolImports;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Coordinations.SchoolImports;

public sealed partial class SchoolImportCoordinationServiceTests
{
    [Fact]
    public void CanImportSchoolRequiresBothOrchestrations()
    {
        // Given
        // When
        // Then
        School school = CreateSchool();

        structureServiceMock
            .Setup(
expression: (ISchoolStructureImportOrchestrationService schoolStructureImportOrchestrationService) =>
                    schoolStructureImportOrchestrationService.CanImportSchool(school: school)
            )
            .Returns(value: true);

        peopleServiceMock
            .Setup(
expression: (ISchoolPeopleImportOrchestrationService schoolPeopleImportOrchestrationService) =>
                    schoolPeopleImportOrchestrationService.CanImportSchool(school: school)
            )
            .Returns(value: true);

        SchoolImportCoordinationService service = CreateSchoolImportCoordinationService();
        bool result = service.CanImportSchool(school: school);

        result.Should()
            .BeTrue(because: "");
    }
}