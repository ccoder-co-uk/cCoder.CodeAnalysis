// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Tests.Fixtures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.StandardElementTypes.ProcessingServices;

[Collection("Sample architecture")]
public sealed class ProcessingServiceTests(SampleArchitectureFixture fixture)
{
    private Architecture Architecture => fixture.Architecture;

    [Fact]
    public void RuleSTXP001EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXP001",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidProcessingDependencyService",
            11
        );
    }

    [Fact]
    public void RuleSTX0003EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0003",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.RedundantProcessingService",
            7
        );
    }

    [Fact]
    public void RuleSTXP002EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXP002",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidStudentService",
            7
        );
    }

    [Fact]
    public void RuleSTXP003EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXP003",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidTeacherProcessingService",
            10
        );
    }

    [Fact]
    public void RuleSTX0013EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0013",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidInterfaceProcessingService",
            7
        );
    }

    [Fact]
    public void RuleSTX0014EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0014",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidContractProcessingService",
            7
        );
    }

    [Fact]
    public void RuleSTX0015EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0015",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidContractSurfaceProcessingService",
            7
        );
    }

    [Fact]
    public void RuleSTX0016EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0016",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidVocabularyProcessingService",
            11
        );
    }

    [Fact]
    public void RuleSTX0017EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0017",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidIdentifierProcessingService",
            5
        );
    }

    [Fact]
    public void RuleSTX0018EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0018",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidMutationNamesProcessingService",
            11
        );
    }

    [Fact]
    public void RuleSTX0019EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0019",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidMutationNamesProcessingService",
            18
        );
    }

    [Fact]
    public void RuleSTX0020EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0020",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidMutationNamesProcessingService",
            25
        );
    }

    [Fact]
    public void RuleSTX0021EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STX0021",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidMutationNamesProcessingService",
            32
        );
    }

    [Fact]
    public void RuleSTXFORMAT001EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXFORMAT001",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidIdentifierProcessingService",
            44
        );
    }

    [Fact]
    public void RuleSTXFORMAT002EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXFORMAT002",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidIdentifierProcessingService",
            18
        );
    }

    [Fact]
    public void RuleSTXFORMAT003EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXFORMAT003",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidIdentifierProcessingService",
            10
        );
    }

    [Fact]
    public void RuleSTXFORMAT004EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXFORMAT004",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidIdentifierProcessingService",
            19
        );
    }

    [Fact]
    public void RuleSTXFORMAT005EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXFORMAT005",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidIdentifierProcessingService",
            21
        );
    }

    [Fact]
    public void RuleSTXSTRUCT001EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXSTRUCT001",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidIdentifierProcessingService",
            7
        );
    }

    [Fact]
    public void RuleSTXFORMAT006EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXFORMAT006",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidIdentifierProcessingService",
            5
        );
    }

    [Fact]
    public void RuleSTXFORMAT007EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXFORMAT007",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidIdentifierProcessingService",
            27
        );
    }

    [Fact]
    public void RuleSTXFORMAT008EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXFORMAT008",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidIdentifierProcessingService",
            34
        );
    }

    [Fact]
    public void RuleSTXFORMAT009EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXFORMAT009",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidIdentifierProcessingService",
            42
        );
    }

    [Fact]
    public void RuleSTXFORMAT010EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXFORMAT010",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidIdentifierProcessingService",
            41
        );
    }

    [Fact]
    public void RuleSTXFORMAT011EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXFORMAT011",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidIdentifierProcessingService",
            1
        );
    }

    [Fact]
    public void RuleSTXFORMAT012EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXFORMAT012",
            "cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations.InvalidProcessingDependencyService",
            13
        );
    }

    [Fact]
    public void ShouldGenerateExpectedNumberOfProcessingServices()
    {
        Count(StandardElementType.ProcessingService).Should().Be(15, "");
    }

    [Fact]
    public void ShouldGenerateValidSchoolImportProcessingService()
    {
        Class element = GetElement(
            "cCoder.CodeAnalysis.Sample.Services.Processings.Schools.SchoolImportProcessingService"
        );
        EnumAssertionsExtensions.Should(element.StandardElementType).Be(StandardElementType.ProcessingService, "");
        element.Methods.Select((Method method) => method.Name).Should().ContainSingle("ImportSchoolAsync");
        ((IEnumerable<AnalysisItem>)Architecture.AnalysisItems)
            .Should()
            .NotContain((AnalysisItem item) => item.Type == element.Name, "");
    }

    [Fact]
    public void ShouldGenerateStudentProcessingService()
    {
        Class element = GetElement("cCoder.CodeAnalysis.Sample.Services.Processings.Students.StudentProcessingService");
        EnumAssertionsExtensions.Should(element.StandardElementType).Be(StandardElementType.ProcessingService, "");
        element
            .Methods.Select((Method method) => method.Name)
            .Should()
            .BeEquivalentTo("AddOrUpdateStudentsAsync", "DeleteStudentsAsync");
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