// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Exposures;
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Tests.Fixtures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Models;

[Collection(DiagnosticSampleParityCollection.Name)]
public sealed class DiagnosticSampleParityTests(
    DiagnosticSampleParityFixture fixture)
{
    [Fact]
    public void EveryCataloguedDiagnosticShouldBeAccountedForByTheSampleSuite()
    {
        string[] cataloguedCodes = DiagnosticCodeStandardPageIndex
            .GetDiagnosticCodeStandardPages()
            .Select(page => page.DiagnosticCode)
            .OrderBy(code => code, StringComparer.Ordinal)
            .ToArray();

        IGrouping<string, SampleDiagnostic>[] demonstratedDiagnostics = fixture
            .Architectures.SelectMany(project => project.Value.AnalysisItems.Select(
                item => new SampleDiagnostic(project.Key, item)))
            .Where(diagnostic => diagnostic.ProjectName ==
                GetIntendedProject(diagnostic.Item.Code))
            .GroupBy(diagnostic => diagnostic.Item.Code, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToArray();

        demonstratedDiagnostics.Should().OnlyContain(
            group => group.Count() == 1,
            "each intentional sample violation must produce exactly one diagnostic across all sample projects");

        string[] accountedForCodes = demonstratedDiagnostics
            .Select(group => group.Key)
            .OrderBy(code => code, StringComparer.Ordinal)
            .ToArray();

        accountedForCodes.Should().OnlyHaveUniqueItems(
            "a demonstrated diagnostic must be removed from the migration list");
        accountedForCodes.Should().Equal(
            cataloguedCodes,
            "every registered diagnostic must remain visible; missing: {0}",
            string.Join(", ", cataloguedCodes.Except(accountedForCodes)));
    }

    [Fact]
    public void ExistingSampleDiagnosticsShouldRemainAssignedToTheirIntendedProjects()
    {
        IReadOnlyDictionary<string, string> demonstratedProjects = fixture
            .Architectures.SelectMany(project => project.Value.AnalysisItems.Select(
                item => new SampleDiagnostic(project.Key, item)))
            .Where(diagnostic => diagnostic.ProjectName ==
                GetIntendedProject(diagnostic.Item.Code))
            .ToDictionary(
                keySelector: diagnostic => diagnostic.Item.Code,
                elementSelector: diagnostic => diagnostic.ProjectName,
                comparer: StringComparer.Ordinal);

        demonstratedProjects.Should().OnlyContain(
            pair => IsAssignedToIntendedProject(pair),
            "rule violations belong in the sample project representing the analysed element type");
    }

    private static bool IsAssignedToIntendedProject(
        KeyValuePair<string, string> diagnostic)
        => diagnostic.Value == GetIntendedProject(diagnostic.Key);

    private static string GetIntendedProject(string diagnosticCode)
    {
        if (diagnosticCode == "STXAPP006")
        {
            return "School.Cli.MissingHost";
        }

        if (diagnosticCode == "STXAPP007")
        {
            return "School.Cli.BadHost";
        }

        if (diagnosticCode.StartsWith("STXAPP", StringComparison.Ordinal))
        {
            return "School.Cli";
        }

        if (diagnosticCode == "STXTEST006")
        {
            return "cCoder.CodeAnalysis.Sample.AcceptanceTests";
        }

        if (diagnosticCode.StartsWith("STXTEST", StringComparison.Ordinal))
        {
            return "cCoder.CodeAnalysis.Sample.Tests";
        }

        if (diagnosticCode.StartsWith("RFC", StringComparison.Ordinal)
            || diagnosticCode.StartsWith("ODATA", StringComparison.Ordinal)
            || diagnosticCode.StartsWith("OWASP", StringComparison.Ordinal))
        {
            return "cCoder.CodeAnalysis.SampleWeb";
        }

        return "cCoder.CodeAnalysis.Sample";
    }

    private sealed record SampleDiagnostic(
        string ProjectName,
        AnalysisItem Item);
}
