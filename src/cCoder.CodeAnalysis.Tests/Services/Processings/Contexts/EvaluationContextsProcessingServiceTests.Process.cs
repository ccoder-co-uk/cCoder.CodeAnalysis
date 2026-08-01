// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Contexts;

public sealed partial class EvaluationContextsProcessingServiceTests
{
    [Fact]
    public void ProcessShouldClassifyHttpMiddlewareAsHttpExposure()
    {
        // Given
        const string source =
            """
            namespace Example;

            public sealed class HttpContext { }
            public delegate void RequestDelegate(HttpContext context);

            public sealed class RequestMiddleware
            {
                public object InvokeAsync(
                    HttpContext context,
                    RequestDelegate next) => new object();
            }

            public sealed class UnrelatedHandler
            {
                public object InvokeAsync(string value) => value;
            }
            """;
        ArchitectureBuild architectureBuild = CreateArchitectureBuild(source: source);

        // When
        EvaluationContext[] contexts = service.Process(architectureBuild).ToArray();

        // Then
        contexts.Single(context => context.TypeName == "Example.RequestMiddleware")
            .StandardElementType.Should().Be(StandardElementType.HttpExposure);
        contexts.Single(context => context.TypeName == "Example.UnrelatedHandler")
            .StandardElementType.Should().Be(StandardElementType.Unknown);
    }

    [Fact]
    public void ProcessShouldExcludeSecurityRequestConfigurationFromLayerDependencies()
    {
        const string source =
            """
            namespace cCoder.Security.Models.Configurations
            {
                public interface ISSOAuthInfo
                {
                    string SSOUserId { get; set; }
                }
            }

            namespace Example.Brokers
            {
                using cCoder.Security.Models.Configurations;

                internal sealed class EventBroker(
                    System.IFormatProvider eventHub,
                    ISSOAuthInfo authInfo)
                {
                }
            }
            """;

        ArchitectureBuild architectureBuild =
            CreateArchitectureBuild(source: source);

        EvaluationContext context = service
            .Process(architectureBuild: architectureBuild)
            .Single(item => item.TypeName == "Example.Brokers.EventBroker");

        context.Dependencies
            .Should()
            .ContainSingle()
            .Which.TypeName.Should()
            .Be("System.IFormatProvider");
    }

    [Fact]
    public void ProcessShouldTreatUnresolvedConstructorDependencyAsUnknownDependency()
    {
        // Given
        const string source =
            """
            namespace Example.Services.Aggregations;

            internal sealed class LocalAggregationService(
                Missing.Dependency dependency)
            {
            }
            """;

        ArchitectureBuild architectureBuild =
            CreateArchitectureBuild(source: source);

        // When
        EvaluationContext context = service
            .Process(architectureBuild: architectureBuild)
            .Single();

        // Then
        TypeDependency dependency = context.Dependencies
            .Should()
            .ContainSingle()
            .Which;

        dependency.StandardElementType.Should()
            .Be(expected: StandardElementType.Dependency);
    }

    [Fact]
    public void ProcessShouldClassifyWorkflowActivityAsActivity()
    {
        // Given
        const string source =
            """
            namespace Example.Activities.Activities.Api;

            public sealed class SendRequestActivity
            {
            }
            """;

        ArchitectureBuild architectureBuild =
            CreateArchitectureBuild(source: source);

        // When
        EvaluationContext context = service
            .Process(architectureBuild: architectureBuild)
            .Single();

        // Then
        context.StandardElementType.Should()
            .Be(expected: StandardElementType.Activity);
    }

    [Fact]
    public void ProcessShouldKeepExternalServiceContractAsDependency()
    {
        // Given
        const string source =
            """
            namespace Example.Services.Aggregations;

            internal sealed class LocalAggregationService(
                System.IFormatProvider externalService)
                : System.IFormatProvider
            {
                public object GetFormat(System.Type formatType) => null;
            }
            """;

        ArchitectureBuild architectureBuild =
            CreateArchitectureBuild(source: source);

        // When
        EvaluationContext context = service
            .Process(architectureBuild: architectureBuild)
            .Single();

        // Then
        TypeDependency dependency = context.Dependencies
            .Should()
            .ContainSingle()
            .Which;

        dependency.TypeName.Should()
            .Be(expected: "System.IFormatProvider");

        dependency.StandardElementType.Should()
            .Be(expected: StandardElementType.Dependency);
    }

    [Fact]
    public void ProcessShouldClassifyReferencedServiceInterfaceAsExposure()
    {
        // Given
        const string referencedSource =
            """
            namespace Referenced.Services.Aggregations;

            public interface IStudentAggregationService
            {
                string GetStudent();
            }
            """;

        const string source =
            """
            namespace Example.Services.Aggregations;

            internal sealed class LocalAggregationService(
                Referenced.Services.Aggregations.IStudentAggregationService studentService)
            {
            }
            """;

        ArchitectureBuild architectureBuild = CreateArchitectureBuild(
            source: source,
            additionalReferences:
            [
                CreateMetadataReference(source: referencedSource),
            ]);

        // When
        EvaluationContext context = service
            .Process(architectureBuild: architectureBuild)
            .Single();

        // Then
        TypeDependency dependency = context.Dependencies
            .Should()
            .ContainSingle()
            .Which;

        dependency.StandardElementType.Should()
            .Be(expected: StandardElementType.Exposure);
    }

    [Fact]
    public void ProcessShouldClassifyBuilderOptionsAsApp()
    {
        // Given
        const string source =
            """
            namespace ExternalServiceTarget;

            public sealed class CoreBuilderOptions
            {
            }
            """;

        ArchitectureBuild architectureBuild =
            CreateArchitectureBuild(source: source);

        // When
        EvaluationContext context = service
            .Process(architectureBuild: architectureBuild)
            .Single();

        // Then
        context.StandardElementType.Should()
            .Be(expected: StandardElementType.App);
    }

    [Fact]
    public void ProcessShouldClassifyStatefulDependencyAsDependency()
    {
        // Given
        const string source =
            """
            namespace Example.Dependencies;

            internal sealed class ScriptDependency
            {
                private readonly System.Net.Http.HttpClient httpClient;

                internal ScriptDependency(
                    System.Net.Http.HttpClient httpClient) =>
                    this.httpClient = httpClient;
            }
            """;

        ArchitectureBuild architectureBuild =
            CreateArchitectureBuild(source: source);

        // When
        EvaluationContext context = service
            .Process(architectureBuild: architectureBuild)
            .Single();

        // Then
        context.StandardElementType.Should()
            .Be(expected: StandardElementType.Dependency);
    }

    [Fact]
    public void ProcessShouldNotTreatTaskAsLeakedExternalResource()
    {
        const string source =
            """
            namespace Example.Dependencies;

            internal sealed class MailClientDependency
                : System.Net.Http.HttpClient
            {
                internal System.Threading.Tasks.Task SendAsync() =>
                    System.Threading.Tasks.Task.CompletedTask;
            }
            """;

        EvaluationContext context = service
            .Process(
                architectureBuild:
                    CreateArchitectureBuild(source: source))
            .Single();

        context.ExposesExternalResource.Should()
            .BeFalse();
    }

    [Fact]
    public void ProcessShouldTreatAwaitedDisposableAsLeakedExternalResource()
    {
        const string source =
            """
            namespace Example.Dependencies;

            internal sealed class MailClientDependency
                : System.Net.Http.HttpClient
            {
                internal System.Threading.Tasks.Task<
                    System.Net.Http.HttpResponseMessage> SendAsync() =>
                    System.Threading.Tasks.Task.FromResult(
                        new System.Net.Http.HttpResponseMessage());
            }
            """;

        EvaluationContext context = service
            .Process(
                architectureBuild:
                    CreateArchitectureBuild(source: source))
            .Single();

        context.ExposesExternalResource.Should()
            .BeTrue();
    }

    [Fact]
    public void ProcessShouldClassifyRootConfigurationMapperAsApp()
    {
        // Given
        const string source =
            """
            namespace ExternalServiceTarget;

            internal static class CoreConfigurationMapper
            {
            }
            """;

        ArchitectureBuild architectureBuild =
            CreateArchitectureBuild(source: source);

        // When
        EvaluationContext context = service
            .Process(architectureBuild: architectureBuild)
            .Single();

        // Then
        context.StandardElementType.Should()
            .Be(expected: StandardElementType.App);
    }

    [Fact]
    public void ProcessShouldClassifyRootUrlResolverAsApp()
    {
        // Given
        const string source =
            """
            namespace ExternalServiceTarget;

            public static class HttpEventHubUrlResolver
            {
            }
            """;

        ArchitectureBuild architectureBuild =
            CreateArchitectureBuild(source: source);

        // When
        EvaluationContext context = service
            .Process(architectureBuild: architectureBuild)
            .Single();

        // Then
        context.StandardElementType.Should()
            .Be(expected: StandardElementType.App);
    }

    [Fact]
    public void ProcessShouldNotClassifyMvcViewControllerAsApiController()
    {
        // Given
        const string source =
            """
            namespace Microsoft.AspNetCore.Mvc
            {
                public class Controller
                {
                }
            }

            namespace Example.Controllers
            {
                public sealed class HomeController
                    : Microsoft.AspNetCore.Mvc.Controller
                {
                }
            }
            """;

        ArchitectureBuild architectureBuild =
            CreateArchitectureBuild(source: source);

        // When
        EvaluationContext context = service
            .Process(architectureBuild: architectureBuild)
            .Single(predicate: context =>
                context.TypeName.EndsWith(
                    value: ".HomeController",
                    comparisonType: StringComparison.Ordinal));

        // Then
        context.IsApiController.Should()
            .BeFalse();
    }

    [Fact]
    public void ProcessShouldClassifyApiNamespaceControllerAsApiController()
    {
        // Given
        const string source =
            """
            namespace Microsoft.AspNetCore.Mvc
            {
                public class Controller
                {
                }
            }

            namespace Example.Controllers.Api
            {
                public sealed class StudentsController
                    : Microsoft.AspNetCore.Mvc.Controller
                {
                }
            }
            """;

        ArchitectureBuild architectureBuild =
            CreateArchitectureBuild(source: source);

        // When
        EvaluationContext context = service
            .Process(architectureBuild: architectureBuild)
            .Single(predicate: context =>
                context.TypeName.EndsWith(
                    value: ".StudentsController",
                    comparisonType: StringComparison.Ordinal));

        // Then
        context.IsApiController.Should()
            .BeTrue();
    }

    [Fact]
    public void ProcessShouldClassifyControllerNamespaceTypeAsApiController()
    {
        // Given
        const string source =
            """
            namespace Example.Controllers
            {
                public sealed class StudentsController
                {
                }
            }
            """;

        ArchitectureBuild architectureBuild =
            CreateArchitectureBuild(source: source);

        // When
        EvaluationContext context = service
            .Process(architectureBuild: architectureBuild)
            .Single();

        // Then
        context.IsApiController.Should()
            .BeTrue();
    }
}
