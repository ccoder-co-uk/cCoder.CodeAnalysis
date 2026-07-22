// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.SchoolImports;

public sealed partial class SchoolStructureImportOrchestrationServiceTests
{
    [Fact]
    public void CanImportSchoolEvaluatesValidAndInvalidSchools()
    {
        // Given
        // When
        // Then
        School validSchool = CreateSchool();
        School invalidSchool = CreateSchool(name:string.Empty);
        SchoolStructureImportOrchestrationService service = CreateSchoolStructureImportOrchestrationService();
        bool validResult = service.CanImportSchool(school:validSchool);
        bool invalidResult = service.CanImportSchool(school:invalidSchool);

        validResult.Should()
            .BeTrue(because:"");

        invalidResult.Should()
            .BeFalse(because:"");
    }
}