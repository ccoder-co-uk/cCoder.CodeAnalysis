// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Coordinations.SchoolImports;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Coordinations.SchoolImports;

public sealed partial class SchoolImportValidationCoordinationServiceTests
{
    private readonly Mock<ISchoolStructureImportOrchestrationService> structureServiceMock =
        new Mock<ISchoolStructureImportOrchestrationService>();

    private readonly Mock<ISchoolPeopleImportOrchestrationService> peopleServiceMock =
        new Mock<ISchoolPeopleImportOrchestrationService>();

    private SchoolImportValidationCoordinationService CreateSchoolImportValidationCoordinationService()
    {
        return new SchoolImportValidationCoordinationService(structureServiceMock.Object, peopleServiceMock.Object);
    }

    private static School CreateSchool()
    {
        return new School { Name = "Test School" };
    }
}