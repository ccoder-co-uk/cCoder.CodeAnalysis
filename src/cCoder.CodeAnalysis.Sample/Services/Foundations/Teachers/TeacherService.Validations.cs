// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.Teachers;

internal sealed partial class TeacherService
{
    private static void Validate(params object?[] inputs)
    {
        if (inputs.Any(predicate: (object? input) => input is null))
        {
            throw new ArgumentNullException(nameof(inputs));
        }
    }

    private static void ValidateTeacherOnGet(int teacherId) =>
        Validate(inputs: teacherId);

    private static void ValidateTeacherOnAdd(Teacher newTeacher) =>
        Validate(inputs: newTeacher);

    private static void ValidateTeacherOnUpdate(Teacher updatedTeacher) =>
        Validate(inputs: updatedTeacher);

    private static void ValidateTeacherOnDelete(int teacherId) =>
        Validate(inputs: teacherId);
}