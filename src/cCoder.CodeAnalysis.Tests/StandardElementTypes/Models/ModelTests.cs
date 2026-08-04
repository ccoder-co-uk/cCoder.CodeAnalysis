// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Tests.Fixtures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.StandardElementTypes.Models;

[Collection("Sample architecture")]
public sealed class ModelTests(SampleArchitectureFixture fixture)
{
    private Architecture Architecture => fixture.Architecture;

    [Fact]
    public void RuleSTXM001EvaluatesAsExpected()
    {
        Architecture.AnalysisItems
            .Should()
            .Contain(
                item =>
                    item.Code == "STXM001"
                    && item.Type == "cCoder.CodeAnalysis.Sample.Models.RuleViolations.InvalidModel"
                    && item.LineNumber == 11,
                "the intentional sample violation must remain represented"
            );
    }

    [Fact]
    public void RuleSTXM002EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected("STXM002", "cCoder.CodeAnalysis.Sample.Models.RuleViolations.InvalidModel", 9);
    }

    [Fact]
    public void RuleSTXM003EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected("STXM003", "cCoder.CodeAnalysis.Sample.Models.RuleViolations.InvalidModel", 9);
    }

    [Fact]
    public void ShouldGenerateExpectedNumberOfModels()
    {
        Count(StandardElementType.Model).Should().Be(12, "");
    }

    [Fact]
    public void ShouldRejectModelObjectOverrides()
    {
        Class element = GetElement("cCoder.CodeAnalysis.Sample.LegacyDataModel");

        Architecture.AnalysisItems
            .Should()
            .ContainSingle(
                item => item.Code == "STXM001" && item.Type == element.Name,
                "object overrides are still methods declared by the model");
    }

    [Fact]
    public void ShouldGenerateStudentModel()
    {
        Class element = GetElement("cCoder.CodeAnalysis.Sample.Models.Schools.Student");
        EnumAssertionsExtensions.Should(element.StandardElementType).Be(StandardElementType.Model, "");
        element
            .Properties.Select((Property property) => property.Name)
            .Should()
            .BeEquivalentTo("Id", "FirstName", "LastName", "SchoolId", "School", "Courses");
        ((IEnumerable<Method>)element.Methods).Should().BeEmpty("");
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