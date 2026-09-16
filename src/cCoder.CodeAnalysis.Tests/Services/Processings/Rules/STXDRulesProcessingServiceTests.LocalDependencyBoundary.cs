// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed partial class STXDRulesProcessingServiceTests
{
    public static TheoryData<string> LocalTypeUsageScenarios => new()
    {
        {
            "private readonly Example.Models.LocalValue value;"
        },
        {
            "public Example.Models.LocalValue Value { get; set; }"
        },
        {
            "public Example.Models.LocalValue Map(Example.Models.LocalValue value) => value;"
        },
        {
            "public object Create() => new Example.Models.LocalValue();"
        },
        {
            "public object Create() => Example.Models.LocalValue.Create();"
        }
    };

    [Theory]
    [MemberData(nameof(LocalTypeUsageScenarios))]
    public void Dependency_WhenUsingAnyLocallyDefinedType_IsReported(
        string localTypeUsage)
    {
        // Given
        EvaluationContext context = CreateExternalApiContext(
            source:
                "namespace Example.Models "
                + "{ public sealed class LocalValue "
                + "{ public static LocalValue Create() => new(); } } "
                + "namespace Example.Dependencies "
                + "{ public sealed class ExternalFrameworkDependency "
                + ": ThirdParty.FrameworkBase "
                + "{ public override void Configure() { } "
                + localTypeUsage
                + " } }",
            externalSource:
                "namespace ThirdParty; public abstract class FrameworkBase "
                + "{ public abstract void Configure(); }",
            typeName: "Example.Dependencies.ExternalFrameworkDependency");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXD007");
    }

    [Fact]
    public void Dependency_WhenDependingOnLocallyDefinedType_IsReported()
    {
        // Given
        EvaluationContext context = CreateExternalApiContext(
            source:
                "namespace Example.Models "
                + "{ public sealed class LocalValue { } } "
                + "namespace Example.Dependencies "
                + "{ public sealed class ExternalFrameworkDependency("
                + "Example.Models.LocalValue value) : ThirdParty.FrameworkBase { } }",
            externalSource:
                "namespace ThirdParty; public abstract class FrameworkBase { }",
            typeName: "Example.Dependencies.ExternalFrameworkDependency");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXD007");
    }

    [Fact]
    public void Dependency_WhenDependingOnAnotherLocalDependency_IsReported()
    {
        // Given
        EvaluationContext context = CreateExternalApiContext(
            source:
                "namespace Example.Dependencies "
                + "{ public sealed class FirstDependency : ThirdParty.FrameworkBase { } "
                + "public sealed class SecondDependency(FirstDependency dependency) "
                + ": ThirdParty.FrameworkBase { } }",
            externalSource:
                "namespace ThirdParty; public abstract class FrameworkBase { }",
            typeName: "Example.Dependencies.SecondDependency");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXD007");
    }

    [Fact]
    public void Dependency_WhenImplementingLocallyDefinedInterface_IsReported()
    {
        // Given
        EvaluationContext context = CreateExternalApiContext(
            source:
                "namespace Example.Dependencies "
                + "{ public interface ILocalContract { } "
                + "public sealed class ExternalFrameworkDependency "
                + ": ThirdParty.FrameworkBase, ILocalContract { } }",
            externalSource:
                "namespace ThirdParty; public abstract class FrameworkBase { }",
            typeName: "Example.Dependencies.ExternalFrameworkDependency");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXD007");
    }

    [Fact]
    public void Dependency_WhenExternalBaseDoesNotRequireAnOverride_IsReported()
    {
        // Given
        EvaluationContext context = CreateExternalApiContext(
            source:
                "namespace Example.Dependencies; "
                + "public sealed class StreamDependency : System.IO.MemoryStream { }",
            externalSource:
                "namespace ThirdParty; public sealed class Marker { }",
            typeName: "Example.Dependencies.StreamDependency");

        context.ArchitectureElement.AnalysisDeclaresDependencyIntent.Should().BeTrue();
        context.ArchitectureElement.AnalysisHasExternalBaseType.Should().BeTrue();

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXD008");
    }

    [Fact]
    public void Dependency_WhenOverridingExternalFrameworkMember_IsAllowed()
    {
        // Given
        EvaluationContext context = CreateExternalApiContext(
            source:
                "namespace Example.Dependencies; "
                + "public sealed class FrameworkDependency : ThirdParty.FrameworkBase "
                + "{ public override void Configure() { } }",
            externalSource:
                "namespace ThirdParty; public abstract class FrameworkBase "
                + "{ public abstract void Configure(); }",
            typeName: "Example.Dependencies.FrameworkDependency");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXD008");
    }
}