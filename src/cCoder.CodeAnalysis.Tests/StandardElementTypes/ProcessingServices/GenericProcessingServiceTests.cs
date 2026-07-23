// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.StandardElementTypes.ProcessingServices;

public sealed class GenericProcessingServiceTests
{
    [Fact]
    public void RuleSTXP003AcceptsGenericEntityNames()
    {
        // given
        const string genericTypeName = "EventProcessingService<T>";

        // when
        string typeName =
            ProcessingServiceCodeAnalysisRulesProcessingService.RemoveGenericTypeArguments(
                typeName: genericTypeName);

        // then
        typeName.Should().Be("EventProcessingService");
    }
}