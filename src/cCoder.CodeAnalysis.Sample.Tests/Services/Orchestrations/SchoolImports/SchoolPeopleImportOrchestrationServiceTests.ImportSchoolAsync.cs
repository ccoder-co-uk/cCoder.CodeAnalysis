// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports;
using cCoder.CodeAnalysis.Sample.Services.Processings.Students;
using cCoder.CodeAnalysis.Sample.Services.Processings.Teachers;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.SchoolImports;

public sealed partial class SchoolPeopleImportOrchestrationServiceTests
{
    [Fact]
    public async Task ImportSchoolAsyncImportsStudentsAndTeachers()
    {
        // Given
        // When
        // Then
        School school = CreateSchool();
        SchoolPeopleImportOrchestrationService service = CreateSchoolPeopleImportOrchestrationService();
        await service.ImportSchoolAsync(school: school);

        studentServiceMock.Verify(
expression: (IStudentProcessingService processing) => processing.AddOrUpdateStudentsAsync(students: school.Students, schoolId: school.Id),
times: Times.Once
        );

        teacherServiceMock.Verify(
expression: (ITeacherProcessingService processing) => processing.AddOrUpdateTeachersAsync(teachers: school.Teachers, schoolId: school.Id),
times: Times.Once
        );
    }
}