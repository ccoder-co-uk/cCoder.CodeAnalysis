// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Tests.Fixtures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.StandardElementTypes.Tests;

[Collection("Sample tests architecture")]
public sealed class TestSuiteTests(SampleTestsArchitectureFixture fixture)
{
    [Fact]
    public void RuleSTXTEST001EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXTEST001",
            "cCoder.CodeAnalysis.Sample.Tests.RuleViolations.InvalidGenericTests"
        );
    }

    [Fact]
    public void RuleSTXTEST002EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXTEST002",
            "cCoder.CodeAnalysis.Sample.Tests.RuleViolations.InvalidInheritedTests"
        );
    }

    [Fact]
    public void RuleSTXTEST003EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected("STXTEST003", "cCoder.CodeAnalysis.Sample.Tests.RuleViolations.InvalidSuite");
    }

    [Fact]
    public void RuleSTXTEST004EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXTEST004",
            "cCoder.CodeAnalysis.Sample.Tests.RuleViolations.InvalidPartialTests"
        );
    }

    [Fact]
    public void RuleSTXTEST005EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXTEST005",
            "cCoder.CodeAnalysis.Sample.Tests.RuleViolations.InvalidGivenWhenThenTests"
        );
    }

    [Fact]
    public void TestProjectShouldNotGenerateArchitectureFile()
    {
        string architectureFilePath = fixture.ArchitectureFilePath;
        bool architectureFileExists = File.Exists(architectureFilePath);
        architectureFileExists.Should().BeFalse("");
    }

    private void AssertRuleEvaluatesAsExpected(string code, string type)
    {
        AnalysisItem analysisItem = fixture
            .Architecture.AnalysisItems.Where((AnalysisItem item) => item.Code == code)
            .Should()
            .ContainSingle("")
            .Which;
        analysisItem.Type.Should().Be(type, "");
    }
}