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
    private readonly Mock<ISchoolImportManagementService> importServiceMock =
        new Mock<ISchoolImportManagementService>();

    private readonly Mock<ISchoolImportReadinessManagementService> readinessServiceMock =
        new Mock<ISchoolImportReadinessManagementService>();

    private SchoolImportAggregationService CreateSchoolImportAggregationService()
    {
        return new SchoolImportAggregationService(importServiceMock.Object, readinessServiceMock.Object);
    }

    private static School CreateSchool()
    {
        return new School { Name = "Test School" };
    }
}