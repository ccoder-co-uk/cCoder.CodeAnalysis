// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Tests.Fixtures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.StandardElementTypes.UnknownElements;

[Collection("Sample architecture")]
public sealed class UnknownElementTests(SampleArchitectureFixture fixture)
{
    private Architecture Architecture => fixture.Architecture;

    [Fact]
    public void RuleSTX0001EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected("STX0001", "cCoder.CodeAnalysis.Sample.RuleViolations.InvalidStandardElement", 7);
    }

    [Fact]
    public void ShouldGenerateExpectedNumberOfUnknownElements()
    {
        Count(StandardElementType.Unknown).Should().Be(4, "");
    }

    private int Count(StandardElementType elementType)
    {
        return Architecture.Classes.Count((Class element) => element.StandardElementType == elementType);
    }

    private void AssertRuleEvaluatesAsExpected(string code, string type, int lineNumber)
    {
        AnalysisItem analysisItem = Architecture
            .AnalysisItems.Where((AnalysisItem item) => item.Code == code)
            .Should()
            .ContainSingle("")
            .Which;
        analysisItem.Type.Should().Be(type, "");
        analysisItem.LineNumber.Should().Be(lineNumber, "");
    }
}
