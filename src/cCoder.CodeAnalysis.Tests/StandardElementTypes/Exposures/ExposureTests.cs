// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Tests.Fixtures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.StandardElementTypes.Exposures;

[Collection("Sample architecture")]
public sealed class ExposureTests(SampleArchitectureFixture fixture)
{
    private const string InvalidExposure = "cCoder.CodeAnalysis.Sample.Exposures.RuleViolations.InvalidExposure";

    private Architecture Architecture => fixture.Architecture;

    private string ArchitectureJson => fixture.ArchitectureJson;

    [Fact]
    public void RuleSTX0002EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0002",
            "cCoder.CodeAnalysis.Sample.Exposures.RuleViolations.InvalidExposure",
            13
        );
    }

    [Fact]
    public void RuleSTXE001EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXE001",
            "cCoder.CodeAnalysis.Sample.Exposures.RuleViolations.InvalidExposure",
            22
        );
    }

    [Fact]
    public void RuleSTXE002EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXE002",
            "cCoder.CodeAnalysis.Sample.Exposures.RuleViolations.InvalidExposure",
            30
        );
    }

    [Fact]
    public void RuleSTXE003EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXE003",
            "cCoder.CodeAnalysis.Sample.Exposures.RuleViolations.InvalidExposure",
            11
        );
    }

    [Fact]
    public void RuleSTXE004EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXE004",
            "cCoder.CodeAnalysis.Sample.Exposures.RuleViolations.InvalidExposure",
            11
        );
    }

    [Fact]
    public void RuleSTXE005EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXE005",
            "cCoder.CodeAnalysis.Sample.Exposures.RuleViolations.InvalidExposure",
            36
        );
    }

    [Fact]
    public void RuleSTX0022EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0022",
            "cCoder.CodeAnalysis.Sample.Exposures.RuleViolations.InvalidExposure",
            15
        );
    }

    [Fact]
    public void ShouldGenerateExpectedNumberOfExposures()
    {
        Count(StandardElementType.Exposure).Should().Be(15, "");
    }

    [Fact]
    public void ShouldNotApplyMutationParameterNamingToServiceCollectionExtensions()
    {
        Architecture
            .AnalysisItems.Should()
            .NotContain(
                item =>
                    item.Code == "STX0019"
                    && item.Type.EndsWith(".IServiceCollectionExtensions", StringComparison.Ordinal),
                "");
    }

    [Fact]
    public void ShouldSerializeStandardElementTypeAsAString()
    {
        ArchitectureJson.Should().Contain("\"StandardElementType\": \"Exposure\"", "");
    }

    [Fact]
    public void ShouldGenerateStudentManager()
    {
        Class element = GetElement("cCoder.CodeAnalysis.Sample.Exposures.Students.StudentManager");
        EnumAssertionsExtensions.Should(element.StandardElementType).Be(StandardElementType.Exposure, "");
        element
            .Methods.Select((Method method) => method.Name)
            .Should()
            .BeEquivalentTo("GetStudent", "GetStudents", "AddStudentAsync", "UpdateStudentAsync", "DeleteStudentAsync");
    }

    [Fact]
    public void ShouldLinkStudentManagerToStudentOrchestrationService()
    {
        ((IEnumerable<Link>)Architecture.Links)
            .Should()
            .ContainEquivalentOf(
                new Link
                {
                    FromType = "cCoder.CodeAnalysis.Sample.Exposures.Students.StudentManager",
                    ToType = "cCoder.CodeAnalysis.Sample.Services.Orchestrations.Students.StudentOrchestrationService",
                },
                ""
            );
    }

    [Fact]
    public void ShouldGenerateExpectedNumberOfLinks()
    {
        ((IEnumerable<Link>)Architecture.Links).Should().HaveCount(68, "");
    }

    [Fact]
    public void ShouldGenerateValidSchoolImportManager()
    {
        Class element = GetElement("cCoder.CodeAnalysis.Sample.Exposures.SchoolImports.SchoolImportManager");
        EnumAssertionsExtensions.Should(element.StandardElementType).Be(StandardElementType.Exposure, "");
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