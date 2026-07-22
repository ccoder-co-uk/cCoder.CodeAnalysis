// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Coordinations.SchoolImports;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Coordinations.SchoolImports;

public sealed partial class SchoolImportValidationCoordinationServiceTests
{
    [Fact]
    public void CanImportSchoolRequiresBothValidationOrchestrations()
    {
        // Given
        // When
        // Then
        School school = CreateSchool();

        structureServiceMock
            .Setup(
expression:                (ISchoolStructureImportOrchestrationService schoolStructureImportOrchestrationService) =>
                    schoolStructureImportOrchestrationService.CanImportSchool(school:school)
            )
            .Returns(value: true);

        peopleServiceMock
            .Setup(
expression:                (ISchoolPeopleImportOrchestrationService schoolPeopleImportOrchestrationService) =>
                    schoolPeopleImportOrchestrationService.CanImportSchool(school:school)
            )
            .Returns(value: true);

        SchoolImportValidationCoordinationService service = CreateSchoolImportValidationCoordinationService();
        bool result = service.CanImportSchool(school:school);

        result.Should()
            .BeTrue(because:"");
    }
}