// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Tests.Exposures.RuleViolations;

public sealed partial class InvalidExposureTests
{
    [Fact]
    public void Exposure_WhenUnitTested_IsInvalid()
    {
        // Given
        bool exposureIsUnitTested = true;

        // When
        bool result = exposureIsUnitTested;

        // Then
        Assert.True(condition: result);
    }
}
