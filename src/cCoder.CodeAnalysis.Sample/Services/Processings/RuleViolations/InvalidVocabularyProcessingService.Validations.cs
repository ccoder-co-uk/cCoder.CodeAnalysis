// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------


namespace cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations;

internal sealed partial class InvalidVocabularyProcessingService
{
    private static void Validate(params object?[] inputs)
    {
        if (inputs.Any(predicate: (object? input) => input is null))
        {
            throw new ArgumentNullException(nameof(inputs));
        }
    }
}