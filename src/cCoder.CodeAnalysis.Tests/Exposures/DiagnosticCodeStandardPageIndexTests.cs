// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Exposures;
using cCoder.CodeAnalysis.Models;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Exposures;

public sealed class DiagnosticCodeStandardPageIndexTests
{
    [Fact]
    public void ShouldIndexEveryDiagnosticCodeOnce()
    {
        DiagnosticCodeStandardPage[] pages = DiagnosticCodeStandardPageIndex
            .GetDiagnosticCodeStandardPages()
            .ToArray();

        pages.Should().HaveCount(121, "");
        pages.Select((DiagnosticCodeStandardPage page) => page.DiagnosticCode).Should().OnlyHaveUniqueItems("");
    }

    [Fact]
    public void ShouldMapStandardRulesToTheRelevantStandardPage()
    {
        DiagnosticCodeStandardPage page = GetDiagnosticCodeStandardPage(diagnosticCode: "STXB004");

        page.StandardPageUris.Should().ContainSingle("").Which.Should().Be(
            "https://github.com/hassanhabib/The-Standard/blob/master/1.%20Brokers/1.%20Brokers.md" +
            "#120-implements-a-local-interface",
            "");
    }

    [Fact]
    public void ShouldMapRulesToAllRelevantStandardPages()
    {
        DiagnosticCodeStandardPage page = GetDiagnosticCodeStandardPage(diagnosticCode: "STXO001");

        page.StandardPageUris.Should().BeEquivalentTo(
            new[]
            {
                "https://github.com/hassanhabib/The-Standard/blob/master/" +
                "2.%20Services/2.3%20Orchestrations/2.3%20Orchestrations.md" +
                "#23210-dependency-balance-florance-pattern",
                "https://github.com/hassanhabib/The-Standard/blob/master/" +
                "2.%20Services/2.3%20Orchestrations/2.3%20Orchestrations.md" +
                "#23211-two-three"
            },
            "");
    }

    [Fact]
    public void ShouldMapEveryRuleToAtLeastOneStandardPage()
    {
        DiagnosticCodeStandardPage[] pages = DiagnosticCodeStandardPageIndex
            .GetDiagnosticCodeStandardPages()
            .ToArray();

        pages.Should().OnlyContain(
            (DiagnosticCodeStandardPage page) => page.StandardPageUris.Count > 0,
            "");
    }

    [Theory]
    [InlineData("RFC0001")]
    [InlineData("RFC0002")]
    [InlineData("RFC0003")]
    [InlineData("RFC0004")]
    [InlineData("RFC0005")]
    [InlineData("RFC0006")]
    [InlineData("RFC0007")]
    [InlineData("RFC0008")]
    [InlineData("RFC0009")]
    [InlineData("RFC0010")]
    [InlineData("RFC0011")]
    [InlineData("RFC0012")]
    [InlineData("RFC0013")]
    [InlineData("RFC0014")]
    [InlineData("RFC0015")]
    public void ShouldMapHttpRulesToAnAuthoritativeSpecification(
        string diagnosticCode)
    {
        DiagnosticCodeStandardPage page =
            GetDiagnosticCodeStandardPage(
                diagnosticCode: diagnosticCode);

        page.StandardPageUris.Should()
            .Contain(
                predicate: uri =>
                    uri.StartsWith(
                        "https://www.rfc-editor.org/rfc/rfc9110.html",
                        StringComparison.Ordinal)
                    || uri.StartsWith(
                        "https://docs.oasis-open.org/odata/",
                        StringComparison.Ordinal));
    }

    [Fact]
    public void ShouldSupportThreeRelevantStandardPages()
    {
        DiagnosticCodeStandardPage page = GetDiagnosticCodeStandardPage(diagnosticCode: "STXSTRUCT001");

        page.StandardPageUris.Should().HaveCount(3, "");
    }

    [Fact]
    public void OneClassPerFileRuleShouldLinkToOrganizationGuidance()
    {
        DiagnosticCodeStandardPage page = GetDiagnosticCodeStandardPage(diagnosticCode: "STXSTRUCT002");

        page.StandardPageUris.Should().HaveCount(3, "");
    }

    private static DiagnosticCodeStandardPage GetDiagnosticCodeStandardPage(string diagnosticCode) =>
        DiagnosticCodeStandardPageIndex
            .GetDiagnosticCodeStandardPages()
            .Single((DiagnosticCodeStandardPage page) => page.DiagnosticCode == diagnosticCode);
}