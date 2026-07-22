// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Runtime.InteropServices;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports;
using cCoder.CodeAnalysis.Sample.Services.Processings.Courses;
using cCoder.CodeAnalysis.Sample.Services.Processings.Schools;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.SchoolImports;

public sealed partial class SchoolStructureImportOrchestrationServiceTests
{
    private readonly Mock<ISchoolImportProcessingService> schoolServiceMock =
        new Mock<ISchoolImportProcessingService>();

    private readonly Mock<ICourseProcessingService> courseServiceMock = new Mock<ICourseProcessingService>();

    private SchoolStructureImportOrchestrationService CreateSchoolStructureImportOrchestrationService()
    {
        return new SchoolStructureImportOrchestrationService(schoolServiceMock.Object, courseServiceMock.Object);
    }

    private static School CreateSchool(string name = "Test School")
    {
        School obj = new School { Name = name };
        int num = 1;
        List<Course> list = new List<Course>(num);
        CollectionsMarshal.SetCount(list:list, count:num);
        CollectionsMarshal.AsSpan(list:list)[0] = new Course { Name = "Mathematics" };
        obj.Courses = list;
        return obj;
    }
}