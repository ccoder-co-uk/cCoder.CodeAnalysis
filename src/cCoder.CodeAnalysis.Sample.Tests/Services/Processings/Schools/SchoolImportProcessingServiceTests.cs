// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Schools;
using cCoder.CodeAnalysis.Sample.Services.Processings.Schools;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Processings.Schools;

public sealed partial class SchoolImportProcessingServiceTests
{
    private readonly Mock<ISchoolService> schoolServiceMock = new Mock<ISchoolService>();

    private SchoolImportProcessingService CreateSchoolImportProcessingService()
    {
        return new SchoolImportProcessingService(schoolServiceMock.Object);
    }

    private static School CreateSchool(int schoolId = 0)
    {
        return new School { Id = schoolId, Name = "Test School" };
    }
}