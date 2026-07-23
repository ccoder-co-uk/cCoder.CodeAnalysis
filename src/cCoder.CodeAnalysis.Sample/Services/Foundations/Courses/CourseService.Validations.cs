// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Processings.Validations;

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.Courses;

internal sealed partial class CourseService
{
    private static void Validate(params object?[] inputs)
    {
        ValidationRulesEngine.Validate(inputs: inputs);
    }

    private static void ValidateCourseOnGet(int courseId) =>
        Validate(inputs: courseId);

    private static void ValidateCourseOnAdd(Course newCourse) =>
        Validate(inputs: newCourse);

    private static void ValidateCourseOnUpdate(Course updatedCourse) =>
        Validate(inputs: updatedCourse);

    private static void ValidateCourseOnDelete(int courseId) =>
        Validate(inputs: courseId);}