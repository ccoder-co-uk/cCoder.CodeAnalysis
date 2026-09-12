// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Foundations.Architectures;
using cCoder.CodeAnalysis.Services.Processings.Architectures;
using cCoder.CodeAnalysis.Services.Processings.Contexts;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Moq;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class DependencyBoundaryRuleGapTests
{
    [Fact]
    public void ExternalDependency_WhenInjectedIntoExposure_IsReported()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Exposures; "
                + "public sealed class StudentEventHandlers(ThirdParty.IEventHub eventHub) "
                + "{ public void ListenToEvents() { } }",
            externalSource:
                "namespace ThirdParty; public interface IEventHub "
                + "{ void ListenToEvent(string name, System.Action handler); }",
            typeName: "Example.Exposures.StudentEventHandlers");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        context.ArchitectureElement.AnalysisDependencies.Should()
            .ContainSingle(
                dependency =>
                    dependency.TypeName == "ThirdParty.IEventHub"
                    && dependency.StandardElementType == StandardElementType.Exposure
                    && !dependency.IsInCurrentProject);

        results.Should().ContainSingle(result => result.Code == "STXD001");
    }

    [Fact]
    public void ExternalDependency_WhenInjectedIntoBroker_IsNotReported()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Brokers; "
                + "public sealed class EventBroker(ThirdParty.IEventHub eventHub) "
                + "{ public void ListenToEvents() { } }",
            externalSource:
                "namespace ThirdParty; public interface IEventHub "
                + "{ void ListenToEvent(string name, System.Action handler); }",
            typeName: "Example.Brokers.EventBroker");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXD001");
    }

    [Fact]
    public void EventHandlerExposure_WhenUsingLocalEventBroker_IsAllowed()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Brokers { public interface IEventBroker { } } "
                + "namespace Example.Exposures.EventHandlers { "
                + "public sealed class StudentEventHandlers(Example.Brokers.IEventBroker eventBroker) "
                + "{ public void ListenToEvents() { } } }",
            typeName: "Example.Exposures.EventHandlers.StudentEventHandlers");

        // When
        AnalysisItem[] results = new STXERulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXE004");
    }

    [Fact]
    public void LoggingDependency_WhenInjectedIntoExposure_IsNotReportedAsBusinessDependency()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Exposures; "
                + "public sealed class StudentManager(Microsoft.Extensions.Logging.ILogger<StudentManager> logger) { }",
            externalSource:
                "namespace Microsoft.Extensions.Logging; public interface ILogger<T> { }",
            typeName: "Example.Exposures.StudentManager");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXD001");
    }

    [Fact]
    public void EventRegistrationExposure_WhenRegisteringMultipleHandlers_DoesNotSequenceBusinessOperations()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Exposures "
                + "{ public sealed class StudentEventHandlers(ThirdParty.EventHub eventHub) "
                + "{ public void ListenToEvents() "
                + "{ eventHub.ListenToEvent(\"student_add\", () => { }); "
                + "eventHub.ListenToEvent(\"student_update\", () => { }); } } }",
            externalSource:
                "namespace ThirdParty; public sealed class EventHub "
                + "{ public void ListenToEvent(string name, System.Action handler) { } }",
            typeName: "Example.Exposures.StudentEventHandlers");

        // When
        AnalysisItem[] results = new STXERulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXE005");
    }

    [Fact]
    public void Exposure_WhenCallingMultipleBusinessOperations_StillReportsSequencing()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Services.Processings "
                + "{ public interface IStudentProcessingService "
                + "{ void Add(); void Update(); } } "
                + "namespace Example.Exposures "
                + "{ public sealed class StudentManager("
                + "Example.Services.Processings.IStudentProcessingService service) "
                + "{ public void Save() { service.Add(); service.Update(); } } }",
            typeName: "Example.Exposures.StudentManager");

        // When
        AnalysisItem[] results = new STXERulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXE005");
    }

    [Fact]
    public void InheritedOrchestrationMethod_WhenCalledInsideCallback_IsAttributedToReceiverService()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Exposures "
                + "{ public interface IStudentManager { void Execute(); } } "
                + "namespace Example.Services.Orchestrations "
                + "{ public interface IStudentOrchestrationService : "
                + "Example.Exposures.IStudentManager { } "
                + "public interface ITeacherOrchestrationService { void Execute(); } } "
                + "namespace Example.Services.Coordinations "
                + "{ public sealed class StudentCoordinationService("
                + "Example.Services.Orchestrations.IStudentOrchestrationService students, "
                + "Example.Services.Orchestrations.ITeacherOrchestrationService teachers) "
                + "{ public void Execute() => Run(() => "
                + "{ students.Execute(); teachers.Execute(); }); "
                + "private static void Run(System.Action operation) => operation(); } }",
            typeName: "Example.Services.Coordinations.StudentCoordinationService");

        // When
        AnalysisItem[] results = new STXCRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXC001");
        context.ArchitectureElement.AnalysisDependencies.Should().OnlyContain(
            dependency => dependency.StandardElementType ==
                StandardElementType.OrchestrationService);
    }

    [Fact]
    public void ExposureDependency_WhenUsedByCoordinationService_IsStillRejected()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Exposures "
                + "{ public interface IStudentManager { void Execute(); } } "
                + "namespace Example.Services.Orchestrations "
                + "{ public interface ITeacherOrchestrationService { void Execute(); } } "
                + "namespace Example.Services.Coordinations "
                + "{ public sealed class StudentCoordinationService("
                + "Example.Exposures.IStudentManager students, "
                + "Example.Services.Orchestrations.ITeacherOrchestrationService teachers) "
                + "{ public void Execute() => Run(() => "
                + "{ students.Execute(); teachers.Execute(); }); "
                + "private static void Run(System.Action operation) => operation(); } }",
            typeName: "Example.Services.Coordinations.StudentCoordinationService");

        // When
        AnalysisItem[] results = new STXCRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXC001");
    }

    [Fact]
    public void ExtensionMethod_WhenCalledOnExistingBrokerDependency_DoesNotAddItsContainer()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Brokers "
                + "{ public sealed class StudentBroker(ThirdParty.ExternalClient client) "
                + "{ public void Execute() => Run(() => client.ExecuteWithExtension()); "
                + "private static void Run(System.Action operation) => operation(); } }",
            externalSource:
                "namespace ThirdParty; public sealed class ExternalClient { } "
                + "public static class ExternalClientExtensions "
                + "{ public static void ExecuteWithExtension(this ExternalClient client) { } }",
            typeName: "Example.Brokers.StudentBroker");

        // When
        AnalysisItem[] results = new STXBRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXB001");
        context.ArchitectureElement.AnalysisDependencies.Should().ContainSingle(
            dependency => dependency.TypeName == "ThirdParty.ExternalClient");
    }

    [Fact]
    public void GenericServiceLocatorTypeParameter_WhenUsedByBroker_IsNotAConcreteDependency()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Brokers "
                + "{ public sealed class StudentBroker(System.IServiceProvider provider) "
                + "{ public T GetRequiredService<T>() where T : notnull "
                + "=> ThirdParty.ServiceProviderExtensions.GetRequiredService<T>(provider); } }",
            externalSource:
                "namespace ThirdParty; public static class ServiceProviderExtensions "
                + "{ public static T GetRequiredService<T>(System.IServiceProvider provider) "
                + "=> default(T); }",
            typeName: "Example.Brokers.StudentBroker");

        // When
        AnalysisItem[] results = new STXBRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXB001");
        context.ArchitectureElement.AnalysisDependencies.Should().NotContain(
            dependency => dependency.TypeName == "T");
    }

    [Fact]
    public void ConcreteServiceLocatorTarget_WhenUsedByBroker_IsStillAConcreteDependency()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Brokers "
                + "{ public sealed class StudentBroker(System.IServiceProvider provider) "
                + "{ public object GetRequiredService() "
                + "=> ThirdParty.ServiceProviderExtensions.GetRequiredService<"
                + "ThirdParty.ExternalClient>(provider); } }",
            externalSource:
                "namespace ThirdParty; public sealed class ExternalClient { } "
                + "public static class ServiceProviderExtensions "
                + "{ public static T GetRequiredService<T>(System.IServiceProvider provider) "
                + "=> default(T); }",
            typeName: "Example.Brokers.StudentBroker");

        // When
        AnalysisItem[] results = new STXBRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXB001");
        context.ArchitectureElement.AnalysisDependencies.Should().Contain(
            dependency => dependency.TypeName == "ThirdParty.ExternalClient");
    }

    [Fact]
    public void MethodCallbackParameter_WhenInvokedByFoundation_IsNotAServiceDependency()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Brokers { public interface IStudentBroker { } } "
                + "namespace Example.Services.Foundations "
                + "{ public sealed class StudentService(Example.Brokers.IStudentBroker broker) "
                + "{ public object Execute(System.Func<object> operation) "
                + "=> Run(() => operation()); "
                + "private static object Run(System.Func<object> operation) => operation(); } }",
            typeName: "Example.Services.Foundations.StudentService");

        // When
        AnalysisItem[] results = new STXFRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXF002");
        context.ArchitectureElement.AnalysisDependencies.Should().NotContain(
            dependency => dependency.TypeName.StartsWith(
                "System.Func", StringComparison.Ordinal));
    }

    [Fact]
    public void ConstructorCallbackDependency_WhenInvokedByFoundation_IsStillRejected()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Services.Foundations "
                + "{ public sealed class StudentService(System.Func<object> operation) "
                + "{ public object Execute() => Run(() => operation()); "
                + "private static object Run(System.Func<object> callback) => callback(); } }",
            typeName: "Example.Services.Foundations.StudentService");

        // When
        AnalysisItem[] results = new STXFRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXF002");
    }

    [Fact]
    public void WrappedHigherLayerDependency_WhenInjectedIntoBroker_IsRejected()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Services.Processings "
                + "{ public interface IStudentProcessingService { } } "
                + "namespace Example.Brokers "
                + "{ public sealed class StudentBroker("
                + "System.Collections.Generic.IEnumerable<"
                + "Example.Services.Processings.IStudentProcessingService> services) { } }",
            typeName: "Example.Brokers.StudentBroker");

        // When
        AnalysisItem[] results = new STXBRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXB006");
    }

    [Fact]
    public void WrappedDependency_WhenInjectedIntoBroker_IsAllowed()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Brokers "
                + "{ public sealed class StudentBroker("
                + "System.Collections.Generic.IEnumerable<"
                + "System.IFormatProvider> dependencies) { } }",
            typeName: "Example.Brokers.StudentBroker");

        // When
        AnalysisItem[] results = new STXBRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXB006");
    }

    [Fact]
    public void HigherLayerCall_WhenInsideRegistrationLambda_IsRejected()
    {
        // Given
        EvaluationContext context = CreateContext(
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
            typeName: "Example.Services.Foundations.StudentService");

        // When
        AnalysisItem[] results = new STXFRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXF002");
    }

    [Fact]
    public void BrokerCall_WhenInsideRegistrationLambda_IsAllowed()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example "
                + "{ public sealed class EventSource "
                + "{ public void Register(System.Action action) { } } } "
                + "namespace Example.Brokers "
                + "{ public static class StudentBroker "
                + "{ public static void Process() { } } } "
                + "namespace Example.Services.Foundations "
                + "{ public sealed class StudentService "
                + "{ public void Register(Example.EventSource source) "
                + "=> source.Register(() => Example.Brokers.StudentBroker.Process()); } }",
            typeName: "Example.Services.Foundations.StudentService");

        // When
        AnalysisItem[] results = new STXFRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXF002");
    }

    [Fact]
    public void HigherLayerService_WhenResolvedFromServiceProvider_IsRejected()
    {
        // Given
        EvaluationContext context = CreateContext(
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
        AnalysisItem[] results = new STXFRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXF002");
    }

    [Fact]
    public void Broker_WhenResolvedFromServiceProvider_IsAllowedByLayerRule()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Brokers "
                + "{ public interface IStudentBroker { } } "
                + "namespace Example.Services.Foundations "
                + "{ public sealed class StudentService "
                + "{ public object Resolve(System.IServiceProvider provider) "
                + "=> ThirdParty.ServiceProviderExtensions.GetRequiredService<"
                + "Example.Brokers.IStudentBroker>(provider); } }",
            externalSource:
                "namespace ThirdParty; public static class ServiceProviderExtensions "
                + "{ public static T GetRequiredService<T>(System.IServiceProvider provider) "
                + "=> default(T); }",
            typeName: "Example.Services.Foundations.StudentService");

        // When
        AnalysisItem[] results = new STXFRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXF002");
    }

    [Fact]
    public void ExternalBaseType_WhenUsedByHttpExposure_IsAllowed()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Controllers; "
                + "public sealed class StudentController : ThirdParty.HttpController { }",
            externalSource:
                "namespace ThirdParty; public abstract class HttpController { }",
            typeName: "Example.Controllers.StudentController");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().NotContain(result =>
            result.Code == "STXD006");
    }

    [Fact]
    public void ExternalBaseType_WhenUsedByFoundationService_IsRejected()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Services.Foundations; "
                + "public sealed class StudentService : ThirdParty.ExternalService { }",
            externalSource:
                "namespace ThirdParty; public abstract class ExternalService { }",
            typeName: "Example.Services.Foundations.StudentService");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXD006");
    }

    [Theory]
    [InlineData("Processings", "StudentProcessingService")]
    [InlineData("Orchestrations", "StudentOrchestrationService")]
    [InlineData("Coordinations", "StudentCoordinationService")]
    [InlineData("Managements", "StudentManagementService")]
    [InlineData("Aggregations", "StudentAggregationService")]
    public void ExternalBaseType_WhenUsedByServiceLayer_IsRejected(
        string serviceLayer,
        string serviceName)
    {
        // Given
        string typeName = $"Example.Services.{serviceLayer}.{serviceName}";

        EvaluationContext context = CreateContext(
            source:
                $"namespace Example.Services.{serviceLayer}; "
                + $"public sealed class {serviceName} : ThirdParty.ExternalService "
                + "{ }",
            externalSource:
                "namespace ThirdParty; public abstract class ExternalService { }",
            typeName: typeName);

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXD006");
    }

    [Fact]
    public void ExternalBaseType_WhenUsedByExposure_IsAllowed()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Exposures; "
                + "public sealed class StudentManager : ThirdParty.ExternalManager { }",
            externalSource:
                "namespace ThirdParty; public abstract class ExternalManager { }",
            typeName: "Example.Exposures.StudentManager");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXD006");
    }

    [Fact]
    public void NewtonsoftCall_WhenMadeAboveBroker_IsReported()
    {
        // Given
        EvaluationContext context = CreateNewtonsoftContext(
            namespaceName: "Example.Services.Foundations",
            methodBody: "=> Newtonsoft.Json.JsonConvert.SerializeObject(value);");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXD005");
    }

    [Fact]
    public void NewtonsoftType_WhenConstructedAboveBroker_IsReported()
    {
        // Given
        EvaluationContext context = CreateNewtonsoftContext(
            namespaceName: "Example.Services.Foundations",
            methodBody: "=> new Newtonsoft.Json.JsonSerializer().Serialize(value);");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().Contain(result => result.Code == "STXD005");
    }

    [Fact]
    public void NewtonsoftUse_WhenMadeByBroker_IsAllowed()
    {
        // Given
        EvaluationContext context = CreateNewtonsoftContext(
            namespaceName: "Example.Brokers",
            methodBody: "=> Newtonsoft.Json.JsonConvert.SerializeObject(value);");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXD005");
    }

    [Fact]
    public void RegexConstruction_WhenMadeAboveBroker_IsReported()
    {
        // Given
        EvaluationContext context = CreateRegexContext(
            namespaceName: "Example.Services.Foundations",
            methodBody:
                "=> new System.Text.RegularExpressions.Regex(pattern).IsMatch(value);");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().Contain(result => result.Code == "STXD005");
    }

    [Fact]
    public void RegexStaticCall_WhenMadeAboveBroker_IsReported()
    {
        // Given
        EvaluationContext context = CreateRegexContext(
            namespaceName: "Example.Services.Foundations",
            methodBody:
                "=> System.Text.RegularExpressions.Regex.IsMatch(value, pattern);");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXD005");
    }

    [Fact]
    public void RegexInstanceCall_WhenMadeAboveBroker_IsReported()
    {
        // Given
        EvaluationContext context = CreateRegexContext(
            namespaceName: "Example.Services.Foundations",
            methodBody:
                "{ System.Text.RegularExpressions.Regex regex = new(pattern); "
                + "return regex.IsMatch(value); }");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().Contain(result => result.Code == "STXD005");
    }

    [Fact]
    public void RegexUse_WhenMadeByBroker_IsAllowed()
    {
        // Given
        EvaluationContext context = CreateRegexContext(
            namespaceName: "Example.Brokers",
            methodBody:
                "=> System.Text.RegularExpressions.Regex.IsMatch(value, pattern);");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXD005");
    }

    [Fact]
    public void SystemTextJsonSerializerCall_WhenMadeAboveBroker_IsReported()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Services.Foundations; "
                + "public sealed class JsonService "
                + "{ public string Serialize(object value) "
                + "=> System.Text.Json.JsonSerializer.Serialize(value); }",
            typeName: "Example.Services.Foundations.JsonService");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXD005");
    }

    [Fact]
    public void SystemTextJsonDocumentCall_WhenMadeAboveBroker_IsReported()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Services.Foundations; "
                + "public sealed class JsonService "
                + "{ public object Parse(string value) "
                + "=> System.Text.Json.JsonDocument.Parse(value); }",
            typeName: "Example.Services.Foundations.JsonService");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXD005");
    }

    [Fact]
    public void SystemTextJsonDeserializeCall_WhenMadeAboveBroker_IsReported()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Services.Foundations; "
                + "public sealed class JsonService "
                + "{ public object Deserialize(string value) "
                + "=> System.Text.Json.JsonSerializer.Deserialize<object>(value); }",
            typeName: "Example.Services.Foundations.JsonService");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXD005");
    }

    [Fact]
    public void SystemTextJsonUse_WhenMadeByBroker_IsAllowed()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Brokers; "
                + "public sealed class JsonBroker "
                + "{ public string Serialize(object value) "
                + "=> System.Text.Json.JsonSerializer.Serialize(value); }",
            typeName: "Example.Brokers.JsonBroker");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXD005");
    }

    [Fact]
    public void ExternalBaseAndInterface_WhenProjected_RetainDistinctReferenceMetadata()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Exposures; "
                + "public sealed class StudentManager : ThirdParty.ExternalManager, "
                + "ThirdParty.IExternalManager { }",
            externalSource:
                "namespace ThirdParty; "
                + "public abstract class ExternalManager { } "
                + "public interface IExternalManager { }",
            typeName: "Example.Exposures.StudentManager");

        // When
        TypeReference? baseType = context.ArchitectureElement.BaseType;
        TypeReference[] interfaces = context.ArchitectureElement.Interfaces.ToArray();

        // Then
        baseType.Should().NotBeNull();
        baseType!.FullName.Should().Be("ThirdParty.ExternalManager");
        baseType.Namespace.Should().Be("ThirdParty");
        baseType.AssemblyName.Should().Be("ThirdParty.External.Library");
        baseType.IsInCurrentProject.Should().BeFalse();

        interfaces.Should().ContainSingle().Which.Should().BeEquivalentTo(
            new TypeReference
            {
                Id = "ThirdParty.External.Library:ThirdParty.IExternalManager",
                FullName = "ThirdParty.IExternalManager",
                Name = "IExternalManager",
                Namespace = "ThirdParty",
                AssemblyName = "ThirdParty.External.Library",
                Kind = ArchitectureTypeKind.Interface,
                IsInCurrentProject = false,
                StandardElementType = StandardElementType.Dependency,
            });
    }

    [Fact]
    public void LocalBaseAndInterface_WhenProjected_AreMarkedInCurrentProject()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Exposures; "
                + "public abstract class StudentManagerBase { } "
                + "public interface IStudentManager { } "
                + "public sealed class StudentManager : StudentManagerBase, IStudentManager { }",
            typeName: "Example.Exposures.StudentManager");

        // When
        TypeReference? baseType = context.ArchitectureElement.BaseType;
        TypeReference[] interfaces = context.ArchitectureElement.Interfaces.ToArray();

        // Then
        baseType.Should().NotBeNull();
        baseType!.IsInCurrentProject.Should().BeTrue();
        interfaces.Should().ContainSingle().Which.IsInCurrentProject.Should().BeTrue();
    }

    [Fact]
    public void ExternalCall_WhenMadeFromConstructor_IsRepresented()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Services.Foundations; "
                + "public sealed class StudentService "
                + "{ public StudentService() { ThirdParty.ExternalApi.Execute(); } }",
            externalSource:
                "namespace ThirdParty; public static class ExternalApi "
                + "{ public static void Execute() { } }",
            typeName: "Example.Services.Foundations.StudentService");

        // When
        MethodCall[] calls = context.ArchitectureElement.AnalysisConstructors
            .SelectMany(method => method.DirectCalls)
            .ToArray();

        // Then
        calls.Should().ContainSingle(call =>
            call.TypeName == "ThirdParty.ExternalApi"
            && call.MethodName == "Execute");
    }

    [Fact]
    public void ExternalCall_WhenMadeFromConstructorAboveBroker_IsReported()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Services.Foundations; "
                + "public sealed class StudentService "
                + "{ public StudentService() { ThirdParty.ExternalApi.Execute(); } }",
            externalSource:
                "namespace ThirdParty; public static class ExternalApi "
                + "{ public static void Execute() { } }",
            typeName: "Example.Services.Foundations.StudentService");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXD005");
    }

    [Fact]
    public void HigherLayerCall_WhenMadeFromConstructor_IsRejected()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Services.Aggregations "
                + "{ public static class StudentAggregationService "
                + "{ public static void Process() { } } } "
                + "namespace Example.Services.Foundations "
                + "{ public sealed class StudentService "
                + "{ public StudentService() "
                + "{ Example.Services.Aggregations.StudentAggregationService.Process(); } } }",
            typeName: "Example.Services.Foundations.StudentService");

        // When
        AnalysisItem[] results = new STXFRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().ContainSingle(result => result.Code == "STXF002");
    }

    [Fact]
    public void ExternalCall_WhenMadeFromPrivateMethod_IsRepresentedAndReported()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Services.Foundations; "
                + "public sealed class StudentService "
                + "{ private void Execute() { ThirdParty.ExternalApi.Execute(); } }",
            externalSource:
                "namespace ThirdParty; public static class ExternalApi "
                + "{ public static void Execute() { } }",
            typeName: "Example.Services.Foundations.StudentService");

        // When
        MethodCall[] calls = context.ArchitectureElement.AnalysisMethods
            .SelectMany(method => method.DirectCalls)
            .ToArray();

        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        calls.Should().ContainSingle(call =>
            call.TypeName == "ThirdParty.ExternalApi"
            && call.MethodName == "Execute");
        results.Should().ContainSingle(result => result.Code == "STXD005");
    }

    [Fact]
    public void HigherLayerCall_WhenMadeFromPrivateRegistrationHelper_IsRejected()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example "
                + "{ public sealed class EventSource "
                + "{ public void Register(System.Action action) { } } } "
                + "namespace Example.Services.Aggregations "
                + "{ public static class StudentAggregationService "
                + "{ public static void Process() { } } } "
                + "namespace Example.Services.Foundations "
                + "{ public sealed class StudentService "
                + "{ private void Register(Example.EventSource source) "
                + "=> source.Register(() => "
                + "Example.Services.Aggregations.StudentAggregationService.Process()); } }",
            typeName: "Example.Services.Foundations.StudentService");

        // When
        MethodCall[] calls = context.ArchitectureElement.AnalysisMethods
            .SelectMany(method => method.DirectCalls)
            .ToArray();

        AnalysisItem[] results = new STXFRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        calls.Should().ContainSingle(call =>
            call.TypeName ==
                "Example.Services.Aggregations.StudentAggregationService");
        results.Should().ContainSingle(result => result.Code == "STXF002");
    }

    [Fact]
    public void ExternalCall_WhenMadeFromBrokerConstructor_IsAllowed()
    {
        // Given
        EvaluationContext context = CreateContext(
            source:
                "namespace Example.Brokers; public sealed class StudentBroker "
                + "{ public StudentBroker() { ThirdParty.ExternalApi.Execute(); } }",
            externalSource:
                "namespace ThirdParty; public static class ExternalApi "
                + "{ public static void Execute() { } }",
            typeName: "Example.Brokers.StudentBroker");

        // When
        AnalysisItem[] results = new STXDRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        // Then
        results.Should().NotContain(result => result.Code == "STXD005");
    }

    private static EvaluationContext CreateNewtonsoftContext(
        string namespaceName,
        string methodBody) =>
        CreateContext(
            source:
                $"namespace {namespaceName}; public sealed class JsonService "
                + "{ public string Serialize(object value) "
                + methodBody
                + " }",
            externalSource:
                "namespace Newtonsoft.Json; "
                + "public static class JsonConvert "
                + "{ public static string SerializeObject(object value) => string.Empty; } "
                + "public sealed class JsonSerializer "
                + "{ public string Serialize(object value) => string.Empty; }",
            typeName: $"{namespaceName}.JsonService");

    private static EvaluationContext CreateRegexContext(
        string namespaceName,
        string methodBody) =>
        CreateContext(
            source:
                $"namespace {namespaceName}; public sealed class RegexService "
                + "{ public bool Matches(string value, string pattern) "
                + methodBody
                + " }",
            typeName: $"{namespaceName}.RegexService");

    private static EvaluationContext CreateContext(
        string source,
        string typeName,
        string? externalSource = null)
    {
        MetadataReference[] additionalReferences = externalSource is null
            ? []
            : [CreateExternalReference(source: externalSource)];

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            text: source,
            path: "DependencyBoundary.cs");

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Example",
            syntaxTrees: [syntaxTree],
            references: [.. GetPlatformReferences(), .. additionalReferences]);

        ArchitectureBuild build = new() { Compilation = compilation };
        Mock<IArchitectureService> architectureServiceMock = new();
        architectureServiceMock.Setup(service => service.Build(compilation)).Returns(build);

        ArchitectureBuild architectureBuild =
            new ArchitectureProcessingService(architectureServiceMock.Object)
                .Process(compilation: compilation);

        return new EvaluationContextsProcessingService()
            .Process(architectureBuild: architectureBuild)
            .Single(context => context.ArchitectureElement.Name == typeName);
    }

    private static MetadataReference CreateExternalReference(string source)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "ThirdParty.External.Library",
            syntaxTrees: [CSharpSyntaxTree.ParseText(text: source)],
            references: GetPlatformReferences(),
            options: new CSharpCompilationOptions(
                outputKind: OutputKind.DynamicallyLinkedLibrary));

        using MemoryStream stream = new();
        EmitResult result = compilation.Emit(peStream: stream);
        result.Success.Should().BeTrue(
            because: string.Join(Environment.NewLine, result.Diagnostics));

        return MetadataReference.CreateFromImage(peImage: stream.ToArray());
    }

    private static MetadataReference[] GetPlatformReferences() =>
        ((string)AppContext.GetData(name: "TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(separator: Path.PathSeparator)
            .Select(selector: path => MetadataReference.CreateFromFile(path: path))
            .ToArray();
}