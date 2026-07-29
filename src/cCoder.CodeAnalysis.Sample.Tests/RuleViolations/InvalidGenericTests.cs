// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Tests.RuleViolations;

public sealed partial class InvalidGenericTests
{
    private static T Echo<T>(T value)
    {
        return value;
    }
}