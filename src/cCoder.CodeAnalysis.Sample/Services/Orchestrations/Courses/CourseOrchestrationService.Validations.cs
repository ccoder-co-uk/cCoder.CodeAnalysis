// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------


namespace cCoder.CodeAnalysis.Sample.Services.Orchestrations.Courses;

internal sealed partial class CourseOrchestrationService
{
    private static void ValidateCourseOnGet(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void ValidateCourseOnAdd(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void ValidateCourseOnUpdate(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void ValidateCourseOnDelete(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void Validate(params object?[] inputs)
    {
        if (inputs.Any(predicate: (object? input) => input is null))
        {
            throw new ArgumentNullException(nameof(inputs));
        }
    }
}