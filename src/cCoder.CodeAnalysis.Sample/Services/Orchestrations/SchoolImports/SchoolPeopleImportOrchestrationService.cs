// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Processings.Students;
using cCoder.CodeAnalysis.Sample.Services.Processings.Teachers;

namespace cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports;

internal sealed partial class SchoolPeopleImportOrchestrationService(
    IStudentProcessingService studentProcessingService,
    ITeacherProcessingService teacherProcessingService
) : ISchoolPeopleImportOrchestrationService
{
    public bool CanImportSchool(School school) =>
        TryCatch(operation: () =>
        {
            Validate(inputs: school);

            return school.Students.All(
                    predicate: (Student student) =>
                        !string.IsNullOrWhiteSpace(value: student.FirstName)
                        && !string.IsNullOrWhiteSpace(value: student.LastName)
                )
                && school.Teachers.All(
                    predicate: (Teacher teacher) =>
                        !string.IsNullOrWhiteSpace(value: teacher.FirstName)
                        && !string.IsNullOrWhiteSpace(value: teacher.LastName)
                );
        });

    public ValueTask ImportSchoolAsync(School school) =>
        TryCatch(operation: async () =>
        {
            Validate(inputs: school);
            await studentProcessingService.AddOrUpdateStudentsAsync(students: school.Students, schoolId: school.Id);
            await teacherProcessingService.AddOrUpdateTeachersAsync(teachers: school.Teachers, schoolId: school.Id);
        });
}