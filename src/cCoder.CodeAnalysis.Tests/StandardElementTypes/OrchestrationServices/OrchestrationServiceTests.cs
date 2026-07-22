// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Tests.Fixtures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.StandardElementTypes.OrchestrationServices;

[Collection("Sample architecture")]
public sealed class OrchestrationServiceTests(SampleArchitectureFixture fixture)
{
    private Architecture Architecture => fixture.Architecture;

    [Fact]
    public void RuleSTXO001EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXO001",
            "cCoder.CodeAnalysis.Sample.Services.Orchestrations.RuleViolations.InvalidOrchestrationService",
            10
        );
    }

    [Fact]
    public void RuleSTXO002EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXO002",
            "cCoder.CodeAnalysis.Sample.Services.Orchestrations.RuleViolations.InvalidSchoolService",
            10
        );
    }

    [Fact]
    public void ShouldGenerateExpectedNumberOfOrchestrationServices()
    {
        Count(StandardElementType.OrchestrationService).Should().Be(8, "");
    }

    [Fact]
    public void ShouldGenerateValidSchoolStructureImportOrchestrationService()
    {
        Class element = GetElement(
            "cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports.SchoolStructureImportOrchestrationService"
        );
        EnumAssertionsExtensions.Should(element.StandardElementType).Be(StandardElementType.OrchestrationService, "");
        element
            .Methods.Select((Method method) => method.Name)
            .Should()
            .BeEquivalentTo("CanImportSchool", "ImportSchoolAsync");
        ((IEnumerable<AnalysisItem>)Architecture.AnalysisItems)
            .Should()
            .NotContain((AnalysisItem item) => item.Type == element.Name, "");
    }

    [Fact]
    public void ShouldGenerateStudentOrchestrationService()
    {
        Class element = GetElement(
            "cCoder.CodeAnalysis.Sample.Services.Orchestrations.Students.StudentOrchestrationService"
        );
        EnumAssertionsExtensions.Should(element.StandardElementType).Be(StandardElementType.OrchestrationService, "");
        element
            .Methods.Select((Method method) => method.Name)
            .Should()
            .BeEquivalentTo("GetStudent", "GetStudents", "AddStudentAsync", "UpdateStudentAsync", "DeleteStudentAsync");
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