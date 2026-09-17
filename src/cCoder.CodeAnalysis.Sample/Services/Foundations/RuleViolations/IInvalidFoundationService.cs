// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations;

internal interface IInvalidFoundationService
{
    void Execute();

    void Perform();

    IEnumerable<Student> GetStudents();

    Student ConvertTeacher(Teacher teacher);
}