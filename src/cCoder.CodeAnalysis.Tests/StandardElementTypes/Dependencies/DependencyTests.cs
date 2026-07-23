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
    public void ShouldNotGenerateDependencyAnalysisItems()
    {
        ((IEnumerable<AnalysisItem>)Architecture.AnalysisItems)
            .Should()
            .NotContain((AnalysisItem item) => item.Type.Contains(".Dependencies.", StringComparison.Ordinal), "");
    }

    [Fact]
    public void ShouldGenerateExpectedNumberOfDependencies()
    {
        Count(StandardElementType.Dependency).Should().Be(8, "");
    }

    [Fact]
    public void ShouldClassifySchoolContextAsDependency()
    {
        Class element = GetElement("cCoder.CodeAnalysis.Sample.Brokers.Storage.SchoolContext");
        EnumAssertionsExtensions.Should(element.StandardElementType).Be(StandardElementType.Dependency, "");
        ((IEnumerable<Property>)element.Properties).Should().HaveCount(4, "");
    }

    [Fact]
    public void ShouldClassifyDependencyNamespaceAsDependency()
    {
        Class element = GetElement("cCoder.CodeAnalysis.Sample.Dependencies.CompatibilityDependency");

        element.StandardElementType.Should().Be(StandardElementType.Dependency, "");
        Architecture.AnalysisItems.Should().NotContain(item => item.Type == element.Name, "");
    }

    [Fact]
    public void ShouldClassifyExternalFrameworkSubclassAsDependency()
    {
        Class element = GetElement("cCoder.CodeAnalysis.Sample.ExternalFrameworkDependency");

        element.StandardElementType.Should().Be(StandardElementType.Dependency, "");
        Architecture.AnalysisItems.Should().NotContain(item => item.Type == element.Name, "");
    }

    [Fact]
    public void ShouldClassifyExtensionContainerAsDependency()
    {
        Class element = GetElement("cCoder.CodeAnalysis.Sample.Extensions.LegacyExtensions");

        element.StandardElementType.Should().Be(StandardElementType.Dependency, "");
        Architecture.AnalysisItems.Should().NotContain(item => item.Type == element.Name, "");
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
