// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Runtime.InteropServices;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports;
using cCoder.CodeAnalysis.Sample.Services.Processings.Students;
using cCoder.CodeAnalysis.Sample.Services.Processings.Teachers;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.SchoolImports;

public sealed partial class SchoolPeopleImportOrchestrationServiceTests
{
    private readonly Mock<IStudentProcessingService> studentServiceMock = new Mock<IStudentProcessingService>();

    private readonly Mock<ITeacherProcessingService> teacherServiceMock = new Mock<ITeacherProcessingService>();

    private SchoolPeopleImportOrchestrationService CreateSchoolPeopleImportOrchestrationService()
    {
        return new SchoolPeopleImportOrchestrationService(studentServiceMock.Object, teacherServiceMock.Object);
    }

    private static School CreateSchool(string studentFirstName = "Ada")
    {
        School obj = new School { Id = 7 };
        int num = 1;
        List<Student> list = new List<Student>(num);
        CollectionsMarshal.SetCount(list:list, count:num);
        CollectionsMarshal.AsSpan(list:list)[0] = new Student { FirstName = studentFirstName, LastName = "Lovelace" };
        obj.Students = list;
        num = 1;
        List<Teacher> list2 = new List<Teacher>(num);
        CollectionsMarshal.SetCount(list:list2, count:num);
        CollectionsMarshal.AsSpan(list:list2)[0] = new Teacher { FirstName = "Grace", LastName = "Hopper" };
        obj.Teachers = list2;
        return obj;
    }
}