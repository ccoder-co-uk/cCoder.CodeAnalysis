// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Coordinations.SchoolImports;
using cCoder.CodeAnalysis.Sample.Services.Managements.SchoolImports;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Managements.SchoolImports;

public sealed partial class SchoolImportManagementServiceTests
{
    [Fact]
    public async Task ImportSchoolAsyncOnlyImportsValidSchool()
    {
        // Given
        // When
        // Then
        School school = CreateSchool();

        validationServiceMock
            .Setup(
expression:                (ISchoolImportValidationCoordinationService schoolImportValidationCoordinationService) =>
                    schoolImportValidationCoordinationService.CanImportSchool(school:school)
            )
            .Returns(value: true);

        SchoolImportManagementService service = CreateSchoolImportManagementService();
        await service.ImportSchoolAsync(school:school);

        importServiceMock.Verify(
expression:            (ISchoolImportCoordinationService coordination) => coordination.ImportSchoolAsync(school:school),
times:            Times.Once
        );
    }
}