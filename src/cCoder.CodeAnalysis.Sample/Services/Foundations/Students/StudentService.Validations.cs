// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Processings.Validations;

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.Students;

internal sealed partial class StudentService
{
    private static void Validate(params object?[] inputs)
    {
        ValidationRulesEngine.Validate(inputs: inputs);
    }

    private static void ValidateStudentOnGet(int studentId) =>
        Validate(inputs: studentId);

    private static void ValidateStudentOnAdd(Student newStudent) =>
        Validate(inputs: newStudent);

    private static void ValidateStudentOnUpdate(Student updatedStudent) =>
        Validate(inputs: updatedStudent);

    private static void ValidateStudentOnDelete(int studentId) =>
        Validate(inputs: studentId);
}