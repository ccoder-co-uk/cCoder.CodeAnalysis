// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Contexts;

public sealed partial class EvaluationContextsProcessingServiceTests
{
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