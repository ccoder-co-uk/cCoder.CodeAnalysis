// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Tests.Fixtures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.StandardElementTypes.ManagementServices;

[Collection("Sample architecture")]
public sealed class ManagementServiceTests(SampleArchitectureFixture fixture)
{
    private Architecture Architecture => fixture.Architecture;

    [Fact]
    public void RuleSTXMG001EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXMG001",
            "cCoder.CodeAnalysis.Sample.Services.Managements.RuleViolations.InvalidManagementService",
            11
        );
    }

    [Fact]
    public void RuleSTXMG002EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXMG002",
            "cCoder.CodeAnalysis.Sample.Services.Managements.RuleViolations.InvalidSchoolService",
            10
        );
    }

    [Fact]
    public void ShouldGenerateExpectedNumberOfManagementServices()
    {
        Count(StandardElementType.ManagementService).Should().Be(4, "");
    }

    [Fact]
    public void ShouldGenerateValidSchoolImportManagementService()
    {
        Class element = GetElement(
            "cCoder.CodeAnalysis.Sample.Services.Managements.SchoolImports.SchoolImportManagementService"
        );
        EnumAssertionsExtensions.Should(element.StandardElementType).Be(StandardElementType.ManagementService, "");
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