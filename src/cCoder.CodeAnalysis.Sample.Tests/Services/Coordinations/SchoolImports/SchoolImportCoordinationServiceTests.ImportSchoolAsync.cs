// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Coordinations.SchoolImports;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Coordinations.SchoolImports;

public sealed partial class SchoolImportCoordinationServiceTests
{
    [Fact]
    public async Task ImportSchoolAsyncInvokesBothOrchestrations()
    {
        // Given
        // When
        // Then
        School school = CreateSchool();
        SchoolImportCoordinationService service = CreateSchoolImportCoordinationService();
        await service.ImportSchoolAsync(school:school);

        structureServiceMock.Verify(
expression:            (ISchoolStructureImportOrchestrationService orchestration) => orchestration.ImportSchoolAsync(school:school),
times:            Times.Once
        );

        peopleServiceMock.Verify(
expression:            (ISchoolPeopleImportOrchestrationService orchestration) => orchestration.ImportSchoolAsync(school:school),
times:            Times.Once
        );
    }
}