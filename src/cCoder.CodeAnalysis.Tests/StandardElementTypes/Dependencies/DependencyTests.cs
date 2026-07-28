// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Tests.Fixtures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.StandardElementTypes.Dependencies;

[Collection("Sample architecture")]
public sealed class DependencyTests(SampleArchitectureFixture fixture)
{
    private Architecture Architecture => fixture.Architecture;

    [Fact]
    public void RuleSTXD001EvaluatesAsExpected()
    {
        AnalysisItem item = Architecture.AnalysisItems
            .Where((AnalysisItem analysisItem) => analysisItem.Code == "STXD001")
            .Should()
            .ContainSingle("")
            .Which;

        item.Type.Should().Be(
            "cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations.InvalidFoundationService",
            "");
        item.LineNumber.Should().Be(10, "");
    }

    [Fact]
    public void ShouldNotGenerateDependencyAnalysisItems()
    {
        ((IEnumerable<AnalysisItem>)Architecture.AnalysisItems)
            .Should()
            .NotContain(
                (AnalysisItem item) =>
                    item.Type == "cCoder.CodeAnalysis.Sample.ExternalFrameworkDependency"
                    || item.Type == "cCoder.CodeAnalysis.Sample.ExternalContractDependency",
                ""
            );
    }

    [Fact]
    public void RuleSTXD002EvaluatesAsExpected()
    {
        Architecture.AnalysisItems
            .Where((AnalysisItem item) => item.Code == "STXD002")
            .Select((AnalysisItem item) => item.Type)
            .Should()
            .ContainSingle("")
            .Which.Should()
            .Be("cCoder.CodeAnalysis.Sample.Dependencies.CompatibilityDependency", "");
    }

    [Fact]
    public void ShouldGenerateExpectedNumberOfDependencies()
    {
        Count(StandardElementType.Dependency).Should().Be(6, "");
    }

    [Fact]
    public void ShouldClassifySchoolContextAsDependency()
    {
        Class element = GetElement("cCoder.CodeAnalysis.Sample.Brokers.Storage.SchoolContext");
        EnumAssertionsExtensions.Should(element.StandardElementType).Be(StandardElementType.Dependency, "");
        ((IEnumerable<Property>)element.Properties).Should().HaveCount(4, "");
    }

    [Fact]
    public void ShouldRejectDependencyNamespaceWithoutExternalContract()
    {
        Class element = GetElement("cCoder.CodeAnalysis.Sample.Dependencies.CompatibilityDependency");

        element.StandardElementType.Should().Be(StandardElementType.Unknown, "");
        Architecture.AnalysisItems.Should().ContainSingle(
            item => item.Code == "STXD002" && item.Type == element.Name,
            ""
        );
    }

    [Fact]
    public void ShouldClassifyExternalFrameworkSubclassAsDependency()
    {
        Class element = GetElement("cCoder.CodeAnalysis.Sample.ExternalFrameworkDependency");

        element.StandardElementType.Should().Be(StandardElementType.Dependency, "");
        Architecture.AnalysisItems.Should().NotContain(item => item.Type == element.Name, "");
    }

    [Fact]
    public void ShouldClassifyExtensionContainerAsExposure()
    {
        Class element = GetElement("cCoder.CodeAnalysis.Sample.Exposures.LegacyExtensions");

        element.StandardElementType.Should().Be(StandardElementType.Exposure, "");
        Architecture.AnalysisItems.Should().NotContain(item => item.Type == element.Name, "");
    }

    [Fact]
    public void ShouldClassifyExternalFrameworkContractAsDependency()
    {
        Class element = GetElement("cCoder.CodeAnalysis.Sample.ExternalContractDependency");

        element.StandardElementType.Should().Be(StandardElementType.Dependency, "");
        Architecture.AnalysisItems.Should().NotContain(item => item.Type == element.Name, "");
    }

    [Fact]
    public void ShouldAcceptDependencyThatImplementsLocalContract()
    {
        Architecture.AnalysisItems.Should().NotContain(
            item =>
                item.Code == "STXD002"
                && item.Type == "cCoder.CodeAnalysis.Sample.Dependencies.LocalContractDependency",
            "");
    }

    [Fact]
    public void ShouldAcceptDependencyThatWrapsExternalState()
    {
        Architecture.AnalysisItems.Should().NotContain(
            item =>
                item.Code == "STXD002"
                && item.Type == "cCoder.CodeAnalysis.Sample.Dependencies.ExternalStateDependency",
            "");
    }

    private Class GetElement(string name)
    {
        return Architecture.Classes.Single((Class element) => element.Name == name);
    }

    private int Count(StandardElementType elementType)
    {
        return Architecture.Classes.Count((Class element) => element.StandardElementType == elementType);
    }
}
