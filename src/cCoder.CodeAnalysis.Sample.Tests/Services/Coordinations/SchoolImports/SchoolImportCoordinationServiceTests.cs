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
    private readonly Mock<ISchoolStructureImportOrchestrationService> structureServiceMock =
        new Mock<ISchoolStructureImportOrchestrationService>();

    private readonly Mock<ISchoolPeopleImportOrchestrationService> peopleServiceMock =
        new Mock<ISchoolPeopleImportOrchestrationService>();

    private SchoolImportCoordinationService CreateSchoolImportCoordinationService()
    {
        return new SchoolImportCoordinationService(structureServiceMock.Object, peopleServiceMock.Object);
    }

    private static School CreateSchool()
    {
        return new School { Name = "Test School" };
    }
}