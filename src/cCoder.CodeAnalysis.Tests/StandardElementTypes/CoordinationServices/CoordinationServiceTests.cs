// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Tests.Fixtures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.StandardElementTypes.CoordinationServices;

[Collection("Sample architecture")]
public sealed class CoordinationServiceTests(SampleArchitectureFixture fixture)
{
    private Architecture Architecture => fixture.Architecture;

    [Fact]
    public void RuleSTXC001EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXC001",
            "cCoder.CodeAnalysis.Sample.Services.Coordinations.RuleViolations.InvalidCoordinationService",
            11
        );
    }

    [Fact]
    public void RuleSTXC002EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXC002",
            "cCoder.CodeAnalysis.Sample.Services.Coordinations.RuleViolations.InvalidSchoolService",
            9
        );
    }

    [Fact]
    public void ShouldGenerateExpectedNumberOfCoordinationServices()
    {
        Count(StandardElementType.CoordinationService).Should().Be(4, "");
    }

    [Fact]
    public void ShouldGenerateValidSchoolImportCoordinationService()
    {
        Class element = GetElement(
            "cCoder.CodeAnalysis.Sample.Services.Coordinations.SchoolImports.SchoolImportCoordinationService"
        );
        EnumAssertionsExtensions.Should(element.StandardElementType).Be(StandardElementType.CoordinationService, "");
        element
            .Methods.Select((Method method) => method.Name)
            .Should()
            .BeEquivalentTo("CanImportSchool", "ImportSchoolAsync");
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