// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations;

internal sealed partial class RedundantProcessingService : IRedundantProcessingService
{
    internal static string Get(string value)
    {
        return value.Trim();
    }
}