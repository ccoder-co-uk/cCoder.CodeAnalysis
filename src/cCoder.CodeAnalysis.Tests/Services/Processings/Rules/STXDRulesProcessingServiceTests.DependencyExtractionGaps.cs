// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed partial class STXDRulesProcessingServiceTests
{
    [Fact]
    public void WrappedServiceDependency_WhenInjectedIntoBroker_IsRepresentedByContainedType()
    {
        // Given
        EvaluationContext context = CreateExternalApiContext(
            source:
                "namespace Example.Services.Processings "
                + "{ public interface IStudentProcessingService { } } "
                + "namespace Example.Brokers "
                + "{ public sealed class StudentBroker("
                + "System.Collections.Generic.IEnumerable<"
                + "Example.Services.Processings.IStudentProcessingService> services) { } }",
            externalSource: "namespace ThirdParty; public sealed class Marker { }",
            typeName: "Example.Brokers.StudentBroker");

        // When
        TypeDependency[] dependencies = context.ArchitectureElement
            .AnalysisDependencies.ToArray();

        // Then
        dependencies.Should().ContainSingle(dependency =>
            dependency.TypeName ==
                "Example.Services.Processings.IStudentProcessingService"
            && dependency.StandardElementType ==
                StandardElementType.ProcessingService);
    }

    [Fact]
    public void HigherLayerCall_WhenInsideRegistrationLambda_IsRepresented()
    {
        // Given
        EvaluationContext context = CreateExternalApiContext(
            source:
                "namespace Example "
                + "{ public sealed class EventSource "
                + "{ public void Register(System.Action action) { } } } "
                + "namespace Example.Services.Aggregations "
                + "{ public static class StudentAggregationService "
                + "{ public static void Process() { } } } "
                + "namespace Example.Services.Foundations "
                + "{ public sealed class StudentService "
                + "{ public void Register(Example.EventSource source) "
                + "=> source.Register(() => "
                + "Example.Services.Aggregations.StudentAggregationService.Process()); } }",
            externalSource: "namespace ThirdParty; public sealed class Marker { }",
            typeName: "Example.Services.Foundations.StudentService");

        // When
        MethodCall[] calls = context.ArchitectureElement.AnalysisMethods
            .SelectMany(method => method.DirectCalls)
            .ToArray();

        // Then
        calls.Should().ContainSingle(call =>
            call.TypeName ==
                "Example.Services.Aggregations.StudentAggregationService"
            && call.MethodName == "Process"
            && call.StandardElementType ==
                StandardElementType.AggregationService);
    }

    [Fact]
    public void ServiceLocatorDependency_WhenResolvedByGenericType_IsRepresented()
    {
        // Given
        EvaluationContext context = CreateExternalApiContext(
            source:
                "namespace Example.Services.Aggregations "
                + "{ public interface IStudentAggregationService { } } "
                + "namespace Example.Services.Foundations "
                + "{ public sealed class StudentService "
                + "{ public object Resolve(System.IServiceProvider provider) "
                + "=> ThirdParty.ServiceProviderExtensions.GetRequiredService<"
                + "Example.Services.Aggregations.IStudentAggregationService>(provider); } }",
            externalSource:
                "namespace ThirdParty; public static class ServiceProviderExtensions "
                + "{ public static T GetRequiredService<T>(System.IServiceProvider provider) "
                + "=> default(T); }",
            typeName: "Example.Services.Foundations.StudentService");

        // When
        TypeDependency[] dependencies = context.ArchitectureElement
            .AnalysisDependencies.ToArray();

        // Then
        dependencies.Should().ContainSingle(dependency =>
            dependency.TypeName ==
                "Example.Services.Aggregations.IStudentAggregationService"
            && dependency.StandardElementType ==
                StandardElementType.AggregationService);
    }

    [Fact]
    public void ServiceLocatorDependencies_WhenMultipleGenericTargetsAreResolved_AreAllRepresented()
    {
        // Given
        EvaluationContext context = CreateExternalApiContext(
            source:
                "namespace Example.Services.Aggregations "
                + "{ public interface IStudentAggregationService { } "
                + "public interface ITeacherAggregationService { } } "
                + "namespace Example.Services.Foundations "
                + "{ public sealed class StudentService "
                + "{ public object[] Resolve(System.IServiceProvider provider) "
                + "=> new object[] { "
                + "ThirdParty.ServiceProviderExtensions.GetRequiredService<"
                + "Example.Services.Aggregations.IStudentAggregationService>(provider), "
                + "ThirdParty.ServiceProviderExtensions.GetRequiredService<"
                + "Example.Services.Aggregations.ITeacherAggregationService>(provider) }; } }",
            externalSource:
                "namespace ThirdParty; public static class ServiceProviderExtensions "
                + "{ public static T GetRequiredService<T>(System.IServiceProvider provider) "
                + "=> default(T); }",
            typeName: "Example.Services.Foundations.StudentService");

        // When
        TypeDependency[] dependencies = context.ArchitectureElement
            .AnalysisDependencies.ToArray();

        // Then
        dependencies.Select(dependency => dependency.TypeName).Should().Contain(
            "Example.Services.Aggregations.IStudentAggregationService",
            "Example.Services.Aggregations.ITeacherAggregationService");
    }
}
