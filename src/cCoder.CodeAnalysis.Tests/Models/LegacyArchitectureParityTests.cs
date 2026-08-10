// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Security.Cryptography;
using System.Text;
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Tests.Fixtures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Models;

[Collection(SampleArchitectureCollection.Name)]
public sealed class LegacyArchitectureParityTests(SampleArchitectureFixture fixture)
{
    private Architecture Architecture => fixture.Architecture;

    [Fact]
    public void SampleShouldProduceEveryLegacyDiagnosticExactlyOnce()
    {
        IGrouping<string, AnalysisItem>[] diagnosticGroups = Architecture
            .AnalysisItems.Where(item => item.Code.StartsWith("STX", StringComparison.Ordinal))
            .GroupBy(item => item.Code, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToArray();

        diagnosticGroups.Should().HaveCount(88, "");
        diagnosticGroups
            .Where(group => group.Key != "STXM001")
            .Should()
            .OnlyContain(group => group.Count() == 1, "");
        diagnosticGroups.Single(group => group.Key == "STXM001").Should().NotBeEmpty("");
    }

    [Fact]
    public void SampleShouldPreserveStandardElementTypeDistribution()
    {
        Dictionary<StandardElementType, int> expected = new()
        {
            [StandardElementType.AggregationService] = 3,
            [StandardElementType.App] = 1,
            [StandardElementType.Broker] = 7,
            [StandardElementType.CoordinationService] = 4,
            [StandardElementType.Dependency] = 9,
            [StandardElementType.Exposure] = 15,
            [StandardElementType.FoundationService] = 8,
            [StandardElementType.HttpExposure] = 5,
            [StandardElementType.ManagementService] = 4,
            [StandardElementType.Model] = 12,
            [StandardElementType.OrchestrationService] = 8,
            [StandardElementType.ProcessingService] = 15,
            [StandardElementType.Unknown] = 4,
        };

        Dictionary<StandardElementType, int> actual = Architecture
            .Classes.GroupBy(element => element.StandardElementType)
            .ToDictionary(group => group.Key, group => group.Count());

        actual.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering(), "");
    }

    [Fact]
    public void SampleShouldPreserveLegacyArchitectureProjection()
    {
        Architecture.Classes.Should().HaveCount(95, "");
        Architecture.Links.Should().HaveCount(75, "");

        CreateClassProjectionHash().Should()
            .Be("3AEB450FA9366DFE6D52CE76AD5E01FBF0C3429AB973F3907B87B72E39482CA6", "");
        CreateLinkProjectionHash().Should()
            .Be("A9D3096DF5A2369C6F795021B477F914EFB2A395D4920C1171298D41255E8AB8", "");
    }

    private string CreateClassProjectionHash()
    {
        IEnumerable<string> lines = Architecture.Classes
            .OrderBy(element => element.Name, StringComparer.Ordinal)
            .Select(element =>
                string.Join(
                    "|",
                    element.Name,
                    element.StandardElementType,
                    string.Join(
                        ",",
                        element.Properties
                            .OrderBy(property => property.Name, StringComparer.Ordinal)
                            .Select(property => $"{property.Name}:{property.Type}")),
                    string.Join(
                        ",",
                        element.Methods
                            .OrderBy(method => method.Name, StringComparer.Ordinal)
                            .ThenBy(method => method.ReturnType, StringComparer.Ordinal)
                            .Select(method =>
                                $"{method.Name}({string.Join(",", method.Inputs.Select(input => $"{input.Name}:{input.Type}"))}):{method.ReturnType}"))));

        return CreateHash(lines);
    }

    private string CreateLinkProjectionHash()
    {
        IEnumerable<string> lines = Architecture.Links
            .Select(link => $"{link.FromType}|{link.ToType}")
            .OrderBy(value => value, StringComparer.Ordinal);

        return CreateHash(lines);
    }

    private static string CreateHash(IEnumerable<string> lines)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(string.Join("\n", lines));
        return Convert.ToHexString(SHA256.HashData(bytes));
    }
}
