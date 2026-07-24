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
}