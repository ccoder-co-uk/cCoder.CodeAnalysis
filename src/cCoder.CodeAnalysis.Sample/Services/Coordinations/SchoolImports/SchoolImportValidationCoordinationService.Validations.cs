// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------


namespace cCoder.CodeAnalysis.Sample.Services.Coordinations.SchoolImports;

internal sealed partial class SchoolImportValidationCoordinationService
{
    private static void Validate(params object?[] inputs)
    {
        if (inputs.Any(predicate: (object? input) => input is null))
        {
            throw new ArgumentNullException(nameof(inputs));
        }
    }
}