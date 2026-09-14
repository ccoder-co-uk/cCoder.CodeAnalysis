// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------


namespace cCoder.CodeAnalysis.Sample.Services.Orchestrations.Teachers;

internal sealed partial class TeacherOrchestrationService
{
    private static void ValidateTeacherOnGet(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void ValidateTeacherOnAdd(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void ValidateTeacherOnUpdate(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void ValidateTeacherOnDelete(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void Validate(params object?[] inputs)
    {
        if (inputs.Any(predicate: (object? input) => input is null))
        {
            throw new ArgumentNullException(nameof(inputs));
        }
    }
}