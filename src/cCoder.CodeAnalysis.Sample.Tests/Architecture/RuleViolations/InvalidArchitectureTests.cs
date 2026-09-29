// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Tests.Architecture.RuleViolations;

public sealed partial class InvalidArchitectureTests
{
    [Fact]
    public void Architecture_WhenLocallyTested_IsInvalid()
    {
        // Given
        bool architectureIsLocallyTested = true;

        // When
        bool result = architectureIsLocallyTested;

        // Then
        Assert.True(condition: result);
    }
}
