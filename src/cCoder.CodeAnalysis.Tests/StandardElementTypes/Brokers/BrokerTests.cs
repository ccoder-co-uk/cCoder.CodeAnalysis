// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Tests.Fixtures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.StandardElementTypes.Brokers;

[Collection("Sample architecture")]
public sealed class BrokerTests(SampleArchitectureFixture fixture)
{
    private const string InvalidBroker = "cCoder.CodeAnalysis.Sample.Brokers.RuleViolations.InvalidBroker";

    private Architecture Architecture => fixture.Architecture;

    [Fact]
    public void RuleSTXB001EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected("STXB001", "cCoder.CodeAnalysis.Sample.Brokers.RuleViolations.InvalidBroker", 9);
    }

    [Fact]
    public void RuleSTXB002EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected("STXB002", "cCoder.CodeAnalysis.Sample.Brokers.RuleViolations.InvalidBroker", 13);
    }

    [Fact]
    public void RuleSTXB003EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected("STXB003", "cCoder.CodeAnalysis.Sample.Brokers.RuleViolations.InvalidBroker", 18);
    }

    [Fact]
    public void RuleSTXB004EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected("STXB004", "cCoder.CodeAnalysis.Sample.Brokers.RuleViolations.InvalidBroker", 9);
    }

    [Fact]
    public void RuleSTXB005EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected("STXB005", "cCoder.CodeAnalysis.Sample.Brokers.RuleViolations.InvalidBroker", 23);
    }

    [Fact]
    public void RuleSTXB006EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected("STXB006", "cCoder.CodeAnalysis.Sample.Brokers.RuleViolations.InvalidBroker", 9);
    }

    [Fact]
    public void RuleSTXB007EvaluatesAsExpected()
    {
        AssertRuleEvaluatesAsExpected(
            "STXB007",
            "cCoder.CodeAnalysis.Sample.Brokers.Storage.RuleViolations.InvalidStorageBroker",
            11
        );
    }

    [Fact]
    public void ShouldNotApplyBrokerRulesToSchoolContext()
    {
        ((IEnumerable<AnalysisItem>)Architecture.AnalysisItems)
            .Should()
            .NotContain(
                (AnalysisItem item) => item.Type == "cCoder.CodeAnalysis.Sample.Brokers.Storage.SchoolContext",
                ""
            );
    }

    [Fact]
    public void ShouldGenerateExpectedNumberOfBrokers()
    {
        Count(StandardElementType.Broker).Should().Be(6, "");
    }

    [Fact]
    public void ShouldGenerateStudentBroker()
    {
        Class element = GetElement("cCoder.CodeAnalysis.Sample.Brokers.Storage.StudentBroker");
        EnumAssertionsExtensions.Should(element.StandardElementType).Be(StandardElementType.Broker, "");
        element
            .Methods.Select((Method method) => method.Name)
            .Should()
            .BeEquivalentTo("SelectAllStudents", "InsertStudentAsync", "UpdateStudentAsync", "DeleteStudentAsync");
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