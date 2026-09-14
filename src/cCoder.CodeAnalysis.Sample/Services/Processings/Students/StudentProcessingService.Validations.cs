// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------


namespace cCoder.CodeAnalysis.Sample.Services.Processings.Students;

internal sealed partial class StudentProcessingService
{
    private static void ValidateOrUpdateStudentsOnAdd(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void ValidateStudentsOnDelete(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void Validate(params object?[] inputs)
    {
        if (inputs.Any(predicate: (object? input) => input is null))
        {
            throw new ArgumentNullException(nameof(inputs));
        }
    }
}