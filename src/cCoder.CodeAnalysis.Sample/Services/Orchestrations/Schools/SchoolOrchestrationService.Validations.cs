// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------


namespace cCoder.CodeAnalysis.Sample.Services.Orchestrations.Schools;

internal sealed partial class SchoolOrchestrationService
{
    private static void ValidateSchoolOnGet(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void ValidateSchoolOnAdd(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void ValidateSchoolOnUpdate(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void ValidateSchoolOnDelete(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void Validate(params object?[] inputs)
    {
        if (inputs.Any(predicate: (object? input) => input is null))
        {
            throw new ArgumentNullException(nameof(inputs));
        }
    }
}