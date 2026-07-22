// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Services.Processings.Validations;

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.Courses;

internal sealed partial class CourseService
{
    private static void Validate(params object?[] inputs)
    {
        ValidationRulesEngine.Validate(inputs: inputs);
    }
}