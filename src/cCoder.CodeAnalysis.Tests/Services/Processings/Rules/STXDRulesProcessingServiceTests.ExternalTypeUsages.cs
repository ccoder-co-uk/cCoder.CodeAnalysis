// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed partial class STXDRulesProcessingServiceTests
{
    [Fact]
    public void ExternalApiType_WhenExposedAsExposureMethodParameter_IsReported()
    {
        // Given
        EvaluationContext context = CreateExternalTypeUsageContext(
            source:
                "namespace Example.Exposures; "
                + "public sealed class JsonManager "
                + "{ public void Process(System.Text.Json.JsonElement element) { } }",
            typeName: "Example.Exposures.JsonManager");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXD005");
    }

    [Fact]
    public void ExternalApiType_WhenUsedInProcessingTypePatternAndLocal_IsReported()
    {
        // Given
        EvaluationContext context = CreateExternalTypeUsageContext(
            source:
                "namespace Example.Services.Processings; "
                + "public sealed class JsonProcessingService "
                + "{ public bool Process(object value) "
                + "{ System.Text.Json.JsonElement local = default; "
                + "return value is System.Text.Json.JsonElement element "
                + "&& element.ValueKind == local.ValueKind; } }",
            typeName: "Example.Services.Processings.JsonProcessingService");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context)
            .ToArray();

        // Then
        results.Should().Contain(result => result.Code == "STXD005");
    }

    [Fact]
    public void ExternalApiType_WhenUsedInFoundationEnumerableLocal_IsReported()
    {
        // Given
        EvaluationContext context = CreateExternalTypeUsageContext(
            source:
                "namespace Example.Services.Foundations; "
                + "public sealed class JsonService "
                + "{ public int Count() "
                + "{ System.Collections.Generic.IEnumerable<System.Text.Json.JsonElement> "
                + "elements = []; return System.Linq.Enumerable.Count(elements); } }",
            typeName: "Example.Services.Foundations.JsonService");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context)
            .ToArray();

        // Then
        results.Should().Contain(result => result.Code == "STXD005");
    }

    [Fact]
    public void ExternalApiType_WhenUsedByBroker_IsAllowed()
    {
        // Given
        EvaluationContext context = CreateExternalTypeUsageContext(
            source:
                "namespace Example.Brokers; "
                + "public sealed class JsonBroker "
                + "{ public System.Text.Json.JsonElement Process("
                + "System.Text.Json.JsonElement element) => element; }",
            typeName: "Example.Brokers.JsonBroker");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXD005");
    }

    [Fact]
    public void BclPrimitiveAndCollectionTypes_WhenUsedAboveBroker_AreAllowed()
    {
        // Given
        EvaluationContext context = CreateExternalTypeUsageContext(
            source:
                "namespace Example.Services.Foundations; "
                + "public sealed class StudentService "
                + "{ public int Count(System.Collections.Generic.IEnumerable<string> values) "
                + "{ System.Collections.Generic.List<string> local = [.. values]; "
                + "return local.Count; } }",
            typeName: "Example.Services.Foundations.StudentService");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXD005");
    }

    private static EvaluationContext CreateExternalTypeUsageContext(
        string source,
        string typeName) =>
        CreateExternalApiContext(
            source: source,
            externalSource: "namespace ThirdParty; public sealed class Marker { }",
            typeName: typeName);
}
