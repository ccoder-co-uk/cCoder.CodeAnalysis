// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Coordinations.SchoolImports;
using cCoder.CodeAnalysis.Sample.Services.Managements.SchoolImports;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Managements.SchoolImports;

public sealed partial class SchoolImportReadinessManagementServiceTests
{
    [Fact]
    public void CanImportSchoolRequiresValidationAndImportReadiness()
    {
        // Given
        // When
        // Then
        School school = CreateSchool();

        validationServiceMock
            .Setup(
expression: (ISchoolImportValidationCoordinationService schoolImportValidationCoordinationService) =>
                    schoolImportValidationCoordinationService.CanImportSchool(school: school)
            )
            .Returns(value: true);

        importServiceMock
            .Setup(
expression: (ISchoolImportCoordinationService schoolImportCoordinationService) =>
                    schoolImportCoordinationService.CanImportSchool(school: school)
            )
            .Returns(value: true);

        SchoolImportReadinessManagementService service = CreateSchoolImportReadinessManagementService();
        bool result = service.CanImportSchool(school: school);

        result.Should()
            .BeTrue(because: "");
    }
}