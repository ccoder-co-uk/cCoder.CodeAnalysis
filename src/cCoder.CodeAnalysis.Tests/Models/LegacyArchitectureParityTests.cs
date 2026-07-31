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

        diagnosticGroups.Should().HaveCount(76, "");
        diagnosticGroups.Should().OnlyContain(group => group.Count() == 1, "");
    }

    [Fact]
    public void SampleShouldPreserveStandardElementTypeDistribution()
    {
        Dictionary<StandardElementType, int> expected = new()
        {
            [StandardElementType.AggregationService] = 3,
            [StandardElementType.App] = 1,
            [StandardElementType.Broker] = 6,
            [StandardElementType.CoordinationService] = 4,
            [StandardElementType.Dependency] = 8,
            [StandardElementType.Exposure] = 16,
            [StandardElementType.FoundationService] = 8,
            [StandardElementType.ManagementService] = 4,
            [StandardElementType.Model] = 10,
            [StandardElementType.OrchestrationService] = 8,
            [StandardElementType.ProcessingService] = 15,
            [StandardElementType.Unknown] = 5,
        };

        Dictionary<StandardElementType, int> actual = Architecture
            .Classes.GroupBy(element => element.StandardElementType)
            .ToDictionary(group => group.Key, group => group.Count());

        actual.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering(), "");
    }

    [Fact]
    public void SampleShouldPreserveLegacyArchitectureProjection()
    {
        Architecture.Classes.Should().HaveCount(88, "");
        Architecture.Links.Should().HaveCount(71, "");

        CreateClassProjectionHash().Should()
            .Be("5A6B11D0A647B0155986C0400B8D782ED0EF8686116D0447AF2160F35539DD28", "");
        CreateLinkProjectionHash().Should()
            .Be("ABC124FD5E99221B2F782E819D29575519909F74D4F1446178F31621D36701F8", "");
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
