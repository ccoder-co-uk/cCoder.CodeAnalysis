// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------


namespace cCoder.CodeAnalysis.Sample.Services.Processings.Courses;

internal sealed partial class CourseProcessingService
{
    private static void ValidateOrUpdateCoursesOnAdd(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void ValidateCoursesOnDelete(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void Validate(params object?[] inputs)
    {
        if (inputs.Any(predicate: (object? input) => input is null))
        {
            throw new ArgumentNullException(nameof(inputs));
        }
    }
}