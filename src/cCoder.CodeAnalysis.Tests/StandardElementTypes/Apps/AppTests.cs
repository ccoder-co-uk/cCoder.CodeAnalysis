// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Tests.Fixtures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.StandardElementTypes.Apps;

[Collection("Sample architecture")]
public sealed class AppTests(SampleArchitectureFixture fixture)
{
    private Architecture Architecture => fixture.Architecture;

    [Fact]
    public void ShouldClassifyServiceCollectionExtensionsAsApp()
    {
        Class element = Architecture.Classes.Single(
            (Class item) => item.Name == "cCoder.CodeAnalysis.Sample.IServiceCollectionExtensions"
        );

        element.StandardElementType.Should().Be(StandardElementType.App, "");
    }

    [Fact]
    public void ShouldGenerateOneAppElement()
    {
        Architecture.Classes.Count(
            (Class item) => item.StandardElementType == StandardElementType.App
        ).Should().Be(1, "");
    }

    [Fact]
    public void ShouldAcceptRootDomainServiceCollectionRegistration()
    {
        Architecture.AnalysisItems.Should().NotContain(
            (AnalysisItem item) =>
                item.Type == "cCoder.CodeAnalysis.Sample.IServiceCollectionExtensions",
            ""
        );
    }
}