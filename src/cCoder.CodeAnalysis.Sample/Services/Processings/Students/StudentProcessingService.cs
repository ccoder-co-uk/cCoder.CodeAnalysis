// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;

namespace cCoder.CodeAnalysis.Sample.Services.Processings.Students;

internal sealed partial class StudentProcessingService(IStudentService studentService) : IStudentProcessingService
{
    public ValueTask AddOrUpdateStudentsAsync(IEnumerable<Student> students, int schoolId) =>
        TryCatch(operation: async () =>
        {
            Validate(inputs: [students, schoolId]);

            foreach (Student student in students)
            {
                student.SchoolId = schoolId;

                if (student.Id == 0)
                {
                    await studentService.AddStudentAsync(newStudent: student);
                }
                else
                {
                    await studentService.UpdateStudentAsync(updatedStudent: student);
                }
            }
        });

    public ValueTask DeleteStudentsAsync(IEnumerable<Student> deletedStudents) =>
        TryCatch(operation: async () =>
        {
            Validate(inputs: deletedStudents);

            foreach (Student deletedStudent in deletedStudents)
            {
                await studentService.DeleteStudentAsync(studentId: deletedStudent.Id);
            }
        });
}