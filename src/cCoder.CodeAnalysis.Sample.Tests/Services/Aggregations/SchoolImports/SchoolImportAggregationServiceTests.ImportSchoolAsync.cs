// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Aggregations.SchoolImports;
using cCoder.CodeAnalysis.Sample.Services.Managements.SchoolImports;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Aggregations.SchoolImports;

public sealed partial class SchoolImportAggregationServiceTests
{
    [Fact]
    public async Task ImportSchoolAsyncOnlyImportsReadySchool()
    {
        // Given
        // When
        // Then
        School school = CreateSchool();

        readinessServiceMock
            .Setup(
expression: (ISchoolImportReadinessManagementService schoolImportReadinessManagementService) =>
                    schoolImportReadinessManagementService.CanImportSchool(school: school)
            )
            .Returns(value: true);

        SchoolImportAggregationService service = CreateSchoolImportAggregationService();
        await service.ImportSchoolAsync(school: school);

        importServiceMock.Verify(
expression: (ISchoolImportManagementService management) => management.ImportSchoolAsync(school: school),
times: Times.Once
        );
    }
}