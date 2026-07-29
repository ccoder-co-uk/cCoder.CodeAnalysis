// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations;

internal interface IInvalidMutationNamesProcessingService
{
    Student AddAsync(Student newStudent);

    Student AddStudent(Student student);

    Student UpdateStudent(Student student);

    Student DeleteStudent(Student student);
}