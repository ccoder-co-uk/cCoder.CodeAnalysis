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
    public void DependencyAdapter_WhenCallingSealedExternalApi_IsClassifiedAndValid()
    {
        // Given
        EvaluationContext context = CreateDependencyAdapterContext(
            source:
                "namespace Example.Models "
                + "{ public sealed class JsonValueDocument(string value) "
                + "{ public string Value { get; } = value; } } "
                + "namespace Example.Dependencies "
                + "{ public sealed class SystemTextJsonDependency "
                + "{ public Example.Models.JsonValueDocument Normalize(object value) "
                + "=> new(System.Text.Json.JsonSerializer.Serialize(value)); } }",
            typeName: "Example.Dependencies.SystemTextJsonDependency");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context)
            .ToArray();

        // Then
        context.ArchitectureElement.StandardElementType
            .Should().Be(StandardElementType.Dependency);
        results.Should().NotContain(result =>
            result.Code == "STXD002"
            || result.Code == "STXD003");
    }

    [Fact]
    public void Broker_WhenConsumingExternalApiDependencyAdapter_IsAllowed()
    {
        // Given
        EvaluationContext context = CreateDependencyAdapterContext(
            source:
                "namespace Example.Dependencies "
                + "{ public sealed class SystemTextJsonDependency "
                + "{ public string Serialize(object value) "
                + "=> System.Text.Json.JsonSerializer.Serialize(value); } } "
                + "namespace Example.Brokers "
                + "{ public interface ISystemTextJsonBroker { } "
                + "public sealed class SystemTextJsonBroker("
                + "Example.Dependencies.SystemTextJsonDependency dependency) "
                + ": ISystemTextJsonBroker "
                + "{ public string Serialize(object value) => dependency.Serialize(value); } }",
            typeName: "Example.Brokers.SystemTextJsonBroker");

        // When
        AnalysisItem[] results = new STXBRulesProcessingService()
            .Evaluate(context)
            .ToArray();

        // Then
        context.ArchitectureElement.AnalysisDependencies
            .Should().ContainSingle(dependency =>
                dependency.TypeName == "Example.Dependencies.SystemTextJsonDependency"
                && dependency.StandardElementType == StandardElementType.Dependency);
        results.Should().NotContain(result => result.Code == "STXB006");
    }

    [Fact]
    public void DependencyAdapter_WhenItHasNoExternalBoundary_RemainsInvalid()
    {
        // Given
        EvaluationContext context = CreateDependencyAdapterContext(
            source:
                "namespace Example.Dependencies; "
                + "public sealed class FakeDependency "
                + "{ public string Execute(string value) => value; }",
            typeName: "Example.Dependencies.FakeDependency");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context)
            .ToArray();

        // Then
        context.ArchitectureElement.StandardElementType
            .Should().Be(StandardElementType.Unknown);
        results.Should().ContainSingle(result => result.Code == "STXD002");
    }

    [Fact]
    public void DependencyAdapter_WhenItLeaksExternalType_RemainsInvalid()
    {
        // Given
        EvaluationContext context = CreateDependencyAdapterContext(
            source:
                "namespace Example.Dependencies; "
                + "public sealed class StreamDependency "
                + "{ public System.IO.MemoryStream Open() => new(); }",
            typeName: "Example.Dependencies.StreamDependency");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXD003");
    }

    private static EvaluationContext CreateDependencyAdapterContext(
        string source,
        string typeName) =>
        CreateExternalApiContext(
            source: source,
            externalSource: "namespace ThirdParty; public sealed class Marker { }",
            typeName: typeName);
}
