// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Tests.Brokers.RuleViolations;

public sealed partial class InvalidBrokerTests
{
    [Fact]
    public void Broker_WhenUnitTested_IsInvalid()
    {
        // Given
        bool brokerIsUnitTested = true;

        // When
        bool result = brokerIsUnitTested;

        // Then
        Assert.True(condition: result);
    }
}
