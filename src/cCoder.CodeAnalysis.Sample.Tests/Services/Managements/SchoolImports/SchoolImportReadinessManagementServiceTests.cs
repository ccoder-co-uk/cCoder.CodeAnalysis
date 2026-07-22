// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Coordinations.SchoolImports;
using cCoder.CodeAnalysis.Sample.Services.Managements.SchoolImports;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Managements.SchoolImports;

public sealed partial class SchoolImportReadinessManagementServiceTests
{
    private readonly Mock<ISchoolImportCoordinationService> importServiceMock =
        new Mock<ISchoolImportCoordinationService>();

    private readonly Mock<ISchoolImportValidationCoordinationService> validationServiceMock =
        new Mock<ISchoolImportValidationCoordinationService>();

    private SchoolImportReadinessManagementService CreateSchoolImportReadinessManagementService()
    {
        return new SchoolImportReadinessManagementService(importServiceMock.Object, validationServiceMock.Object);
    }

    private static School CreateSchool()
    {
        return new School { Name = "Test School" };
    }
}