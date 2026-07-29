// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Models.RuleViolations;

internal sealed class InvalidModel
{
    public required string Name { get; set; } = string.Empty;

    public static void Execute()
    {
    }
}