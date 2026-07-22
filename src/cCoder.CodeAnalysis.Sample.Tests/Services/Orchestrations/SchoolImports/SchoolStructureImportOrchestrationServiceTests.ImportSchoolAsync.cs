// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports;
using cCoder.CodeAnalysis.Sample.Services.Processings.Courses;
using cCoder.CodeAnalysis.Sample.Services.Processings.Schools;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.SchoolImports;

public sealed partial class SchoolStructureImportOrchestrationServiceTests
{
    [Fact]
    public async Task ImportSchoolAsyncImportsSchoolThenCourses()
    {
        // Given
        // When
        // Then
        School school = CreateSchool();
        SchoolStructureImportOrchestrationService service = CreateSchoolStructureImportOrchestrationService();
        await service.ImportSchoolAsync(school:school);

        schoolServiceMock.Verify(
expression:            (ISchoolImportProcessingService processing) => processing.ImportSchoolAsync(school:school),
times:            Times.Once
        );

        courseServiceMock.Verify(
expression:            (ICourseProcessingService processing) => processing.AddOrUpdateCoursesAsync(courses:school.Courses, schoolId:school.Id),
times:            Times.Once
        );
    }
}