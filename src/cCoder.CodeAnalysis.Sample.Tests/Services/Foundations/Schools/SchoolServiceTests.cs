// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Schools;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Schools;

public sealed partial class SchoolServiceTests
{
    private readonly Mock<ISchoolBroker> schoolBrokerMock = new Mock<ISchoolBroker>();

    private SchoolService CreateSchoolService()
    {
        return new SchoolService(schoolBrokerMock.Object);
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