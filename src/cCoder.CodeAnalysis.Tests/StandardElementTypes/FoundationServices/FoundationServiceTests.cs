// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Tests.Fixtures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.StandardElementTypes.FoundationServices;

[Collection("Sample architecture")]
public sealed class FoundationServiceTests(SampleArchitectureFixture fixture)
{
    private const string InvalidFoundationService =
        "cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations.InvalidFoundationService";

    private Architecture Architecture => fixture.Architecture;

    [Fact]
    public void RuleSTXF001EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXF001",
            "cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations.InvalidFoundationService",
            26
        );
    }

    [Fact]
    public void RuleSTXF002EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXF002",
            "cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations.InvalidFoundationService",
            10
        );
    }

    [Fact]
    public void RuleSTX0004EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0004",
            "cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations.InvalidFoundationService",
            10
        );
    }

    [Fact]
    public void RuleSTX0005EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0005",
            "cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations.InvalidFoundationService",
            21
        );
    }

    [Fact]
    public void RuleSTX0006EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0006",
            "cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations.InvalidFoundationService",
            10
        );
    }

    [Fact]
    public void RuleSTX0007EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0007",
            "cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations.InvalidFoundationService",
            10
        );
    }

    [Fact]
    public void RuleSTX0008EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0008",
            "cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations.InvalidFoundationService",
            10
        );
    }

    [Fact]
    public void RuleSTX0009EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0009",
            "cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations.InvalidFoundationService",
            10
        );
    }

    [Fact]
    public void RuleSTX0010EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0010",
            "cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations.InvalidFoundationService",
            10
        );
    }

    [Fact]
    public void RuleSTX0011EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0011",
            "cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations.InvalidFoundationService",
            10
        );
    }

    [Fact]
    public void RuleSTX0012EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0012",
            "cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations.InvalidAtomicFoundationService",
            10
        );
    }

    [Fact]
    public void RuleSTX0023EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0023",
            "cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations.InvalidAtomicFoundationService",
            10
        );
    }

    [Fact]
    public void RuleSTXF003EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXF003",
            "cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations.InvalidAtomicFoundationService",
            17
        );
    }

    [Fact]
    public void ShouldRecogniseEntityEventServiceMappingWork()
    {
        ((IEnumerable<AnalysisItem>)Architecture.AnalysisItems)
            .Should()
            .NotContain(
                (AnalysisItem item) =>
                    item.Code == "STX0003"
                    && item.Type == "cCoder.CodeAnalysis.Sample.Services.Foundations.Events.EntityEventService",
                ""
            );
    }

    [Fact]
    public void ShouldGenerateExpectedNumberOfFoundationServices()
    {
        Count(StandardElementType.FoundationService).Should().Be(8, "");
    }

    [Fact]
    public void ShouldGenerateStudentService()
    {
        Class element = GetElement("cCoder.CodeAnalysis.Sample.Services.Foundations.Students.StudentService");
        EnumAssertionsExtensions.Should(element.StandardElementType).Be(StandardElementType.FoundationService, "");
        element
            .Methods.Select((Method method) => method.Name)
            .Should()
            .BeEquivalentTo("GetStudent", "GetStudents", "AddStudentAsync", "UpdateStudentAsync", "DeleteStudentAsync");
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
