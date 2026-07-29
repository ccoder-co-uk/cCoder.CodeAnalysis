// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;

namespace cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations;

internal sealed partial class InvalidTeacherProcessingService(IStudentService studentService) : IInvalidTeacherProcessingService
{
    public int CountStudents()
    {
        IQueryable<Student> students = TryCatch(operation: () =>
        {
            IQueryable<Student> selectedStudents = studentService.GetStudents();
            return selectedStudents;
        });

        return students.Count();
    }
}