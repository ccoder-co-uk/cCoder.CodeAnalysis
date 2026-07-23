// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Tests.Fixtures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.StandardElementTypes.Tests;

[Collection("SampleAcceptanceTestsArchitectureCollection")]
public sealed class AcceptanceTestSuiteTests(SampleAcceptanceTestsArchitectureFixture fixture)
{
    [Fact]
    public void RuleSTXTEST006EvaluatesAsExpected()
    {
        AnalysisItem analysisItem = fixture
            .Architecture.AnalysisItems.Where((AnalysisItem item) => item.Code == "STXTEST006")
            .Should()
            .ContainSingle("")
            .Which;
        analysisItem
            .Type.Should()
            .Be(
                "cCoder.CodeAnalysis.Sample.AcceptanceTests.RuleViolations.InvalidStudentControllerAcceptanceTests",
                "");
    }
}