// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations;

internal sealed partial class InvalidContractProcessingService : IIncorrectProcessingContract
{
    public int Calculate(int value)
=>
        TryCatch(operation: () =>
        {
            Validate(inputs: [value]);
            return value * 3;
        });
}