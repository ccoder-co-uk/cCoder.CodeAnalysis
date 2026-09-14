// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------


namespace cCoder.CodeAnalysis.Sample.Services.Processings.Teachers;

internal sealed partial class TeacherProcessingService
{
    private static void ValidateOrUpdateTeachersOnAdd(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void ValidateTeachersOnDelete(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void Validate(params object?[] inputs)
    {
        if (inputs.Any(predicate: (object? input) => input is null))
        {
            throw new ArgumentNullException(nameof(inputs));
        }
    }
}