// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Processings.Courses;
using cCoder.CodeAnalysis.Sample.Services.Processings.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports;

internal sealed partial class SchoolStructureImportOrchestrationService(
    ISchoolImportProcessingService schoolProcessingService,
    ICourseProcessingService courseProcessingService
) : ISchoolStructureImportOrchestrationService
{
    public bool CanImportSchool(School school) =>
        TryCatch(operation: () =>
        {
            Validate(inputs: school);

            return !string.IsNullOrWhiteSpace(value: school.Name)
                && school.Courses.All(predicate: (Course course) => !string.IsNullOrWhiteSpace(value: course.Name));
        });

    public ValueTask ImportSchoolAsync(School school) =>
        TryCatch(operation: async () =>
        {
            Validate(inputs: school);
            await schoolProcessingService.ImportSchoolAsync(school: school);
            await courseProcessingService.AddOrUpdateCoursesAsync(courses: school.Courses, schoolId: school.Id);
        });
}