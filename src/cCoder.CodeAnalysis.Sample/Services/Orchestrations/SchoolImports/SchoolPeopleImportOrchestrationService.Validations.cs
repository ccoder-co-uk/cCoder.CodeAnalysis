// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Services.Processings.Validations;

namespace cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports;

internal sealed partial class SchoolPeopleImportOrchestrationService
{
    private static void Validate(params object?[] inputs)
    {
        ValidationRulesEngine.Validate(inputs: inputs);
    }
}