// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Tests.RuleViolations;

public sealed partial class InvalidSuite
{
    [Fact]
    public void TestSuiteCanBeEvaluated()
    {
        // Given
        bool expected = true;

        // When
        bool actual = expected;

        // Then
        Assert.True(condition: actual);
    }
}