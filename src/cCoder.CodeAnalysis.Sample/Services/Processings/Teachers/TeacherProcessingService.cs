// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Teachers;

namespace cCoder.CodeAnalysis.Sample.Services.Processings.Teachers;

internal sealed partial class TeacherProcessingService(ITeacherService teacherService) : ITeacherProcessingService
{
    public ValueTask AddOrUpdateTeachersAsync(IEnumerable<Teacher> teachers, int schoolId) =>
        TryCatch(operation: async () =>
        {
            Validate(inputs: [teachers, schoolId]);

            foreach (Teacher teacher in teachers)
            {
                teacher.SchoolId = schoolId;

                if (teacher.Id == 0)
                {
                    await teacherService.AddTeacherAsync(newTeacher: teacher);
                }
                else
                {
                    await teacherService.UpdateTeacherAsync(updatedTeacher: teacher);
                }
            }
        });

    public ValueTask DeleteTeachersAsync(IEnumerable<Teacher> deletedTeachers) =>
        TryCatch(operation: async () =>
        {
            Validate(inputs: deletedTeachers);

            foreach (Teacher deletedTeacher in deletedTeachers)
            {
                await teacherService.DeleteTeacherAsync(teacherId: deletedTeacher.Id);
            }
        });
}