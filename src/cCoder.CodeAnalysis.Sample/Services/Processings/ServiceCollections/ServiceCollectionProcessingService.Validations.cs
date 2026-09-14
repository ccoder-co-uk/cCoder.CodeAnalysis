// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------


namespace cCoder.CodeAnalysis.Sample.Services.Processings.ServiceCollections;

internal sealed partial class ServiceCollectionProcessingService
{
    private static void ValidateCodeAnalysisSampleOnAdd(params object?[] inputs) =>
        Validate(inputs: inputs);

    private static void Validate(params object?[] inputs)
    {
        if (inputs.Any(predicate: (object? input) => input is null))
        {
            throw new ArgumentNullException(nameof(inputs));
        }
    }
}