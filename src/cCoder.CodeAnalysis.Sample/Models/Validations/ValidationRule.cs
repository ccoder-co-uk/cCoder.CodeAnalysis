// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Models.Validations;

internal sealed class ValidationRule
{
    internal required Func<bool> IsInvalid { get; init; }

    internal required Func<Exception> CreateException { get; init; }
}