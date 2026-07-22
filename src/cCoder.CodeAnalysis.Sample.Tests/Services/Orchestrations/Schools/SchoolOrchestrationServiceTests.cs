// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Schools;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.Schools;

public sealed partial class SchoolOrchestrationServiceTests
{
    private readonly Mock<ISchoolService> schoolServiceMock = new Mock<ISchoolService>();

    private readonly Mock<IEntityEventService> eventServiceMock = new Mock<IEntityEventService>();

    private SchoolOrchestrationService CreateSchoolOrchestrationService()
    {
        return new SchoolOrchestrationService(schoolServiceMock.Object, eventServiceMock.Object);
    }

    private static School CreateSchool(int schoolId = 7)
    {
        return new School { Id = schoolId };
    }

    private static IQueryable<School> CreateSchools(School? school = null)
    {
        return new School[1] { school ?? CreateSchool() }.AsQueryable();
    }
}