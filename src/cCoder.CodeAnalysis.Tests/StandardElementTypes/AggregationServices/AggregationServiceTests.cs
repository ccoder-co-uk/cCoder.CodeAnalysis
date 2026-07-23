// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Tests.Fixtures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.StandardElementTypes.AggregationServices;

[Collection("Sample architecture")]
public sealed class AggregationServiceTests(SampleArchitectureFixture fixture)
{
    private Architecture Architecture => fixture.Architecture;

    [Fact]
    public void RuleSTXA001EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXA001",
            "cCoder.CodeAnalysis.Sample.Services.Aggregations.RuleViolations.InvalidAggregationService",
            12
        );
    }

    [Fact]
    public void RuleSTXA002EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXA002",
            "cCoder.CodeAnalysis.Sample.Services.Aggregations.RuleViolations.InvalidSchoolService",
            10
        );
    }

    [Fact]
    public void ShouldGenerateExpectedNumberOfAggregationServices()
    {
        Count(StandardElementType.AggregationService).Should().Be(3, "");
    }

    [Fact]
    public void ShouldGenerateValidSchoolImportAggregationService()
    {
        Class element = GetElement(
            "cCoder.CodeAnalysis.Sample.Services.Aggregations.SchoolImports.SchoolImportAggregationService"
        );
        EnumAssertionsExtensions.Should(element.StandardElementType).Be(StandardElementType.AggregationService, "");
        element.Methods.Select((Method method) => method.Name).Should().ContainSingle("ImportSchoolAsync");
        ((IEnumerable<AnalysisItem>)Architecture.AnalysisItems)
            .Should()
            .NotContain((AnalysisItem item) => item.Type == element.Name, "");
    }

    private Class GetElement(string name)
    {
        return Architecture.Classes.Single((Class element) => element.Name == name);
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
