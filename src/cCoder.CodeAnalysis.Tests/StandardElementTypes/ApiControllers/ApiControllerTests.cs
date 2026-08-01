// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Tests.Fixtures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.StandardElementTypes.ApiControllers;

[Collection("Sample architecture")]
public sealed class ApiControllerTests(SampleArchitectureFixture fixture)
{
    private const string InvalidController =
        "cCoder.CodeAnalysis.Sample.Controllers.RuleViolations.InvalidStudentsController";

    private Architecture Architecture => fixture.Architecture;

    [Fact]
    public void RuleSTXAPI001EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXAPI001",
            "cCoder.CodeAnalysis.Sample.Controllers.RuleViolations.InvalidStudentsController",
            12
        );
    }

    [Fact]
    public void RuleSTXAPI002EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXAPI002",
            "cCoder.CodeAnalysis.Sample.Controllers.RuleViolations.InvalidStudentsController",
            12
        );
    }

    [Fact]
    public void RuleSTXAPI003EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXAPI003",
            "cCoder.CodeAnalysis.Sample.Controllers.RuleViolations.InvalidStudentsEndpoint",
            11
        );
    }

    [Fact]
    public void RuleSTXAPI004EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXAPI004",
            "cCoder.CodeAnalysis.Sample.Controllers.RuleViolations.InvalidStudentsActionController",
            15
        );
    }

    [Fact]
    public void RuleSTXAPI005EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXAPI005",
            "cCoder.CodeAnalysis.Sample.Controllers.RuleViolations.InvalidStudentsActionController",
            16
        );
    }

    [Fact]
    public void ShouldGenerateStudentsControllerAsAnExposure()
    {
        Class controller = GetElement("cCoder.CodeAnalysis.Sample.Controllers.StudentsController");
        EnumAssertionsExtensions.Should(controller.StandardElementType).Be(StandardElementType.HttpExposure, "");
    }

    [Fact]
    public void ShouldGenerateSchoolImportControllerAsAValidExposure()
    {
        Class controller = GetElement("cCoder.CodeAnalysis.Sample.Controllers.SchoolImportController");
        EnumAssertionsExtensions.Should(controller.StandardElementType).Be(StandardElementType.HttpExposure, "");
        ((IEnumerable<AnalysisItem>)Architecture.AnalysisItems)
            .Should()
            .NotContain((AnalysisItem item) => item.Type == controller.Name, "");
    }

    private Class GetElement(string name)
    {
        return Architecture.Classes.Single((Class element) => element.Name == name);
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