// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Analyzers;
using cCoder.CodeAnalysis.Models;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace cCoder.CodeAnalysis.Tests.Services.Foundations.Architectures;

public sealed class ArchitectureServiceTests
{
    [Fact]
    public void BuildShouldClassifyWebApplicationExtensionsAsApp()
    {
        // Given
        const string source = """
            namespace Sample;

            public sealed class WebApplication
            {
            }

            public static class WebApplicationExtensions
            {
                public static WebApplication Start(this WebApplication application) =>
                    application;
            }
            """;
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            text: source,
            path: "WebApplicationExtensions.cs");
        MetadataReference runtimeReference = MetadataReference.CreateFromFile(
            path: typeof(object).Assembly.Location);
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Sample",
            syntaxTrees: [syntaxTree],
            references: [runtimeReference]);

        // When
        Architecture architecture = ArchitectureAnalysis.Generate(
            compilation: compilation);

        // Then
        Class webApplicationExtensions = architecture.Classes
            .Single(predicate: element => element.Name == "Sample.WebApplicationExtensions");

        webApplicationExtensions.StandardElementType
            .Should()
            .Be(expected: StandardElementType.App);
    }

    [Fact]
    public void BuildShouldHandleUnresolvedPublicApiModelTypes()
    {
        // Given
        const string source = """
            namespace Sample.Exposures;

            public sealed class SampleController(MissingDependency missingDependency)
            {
                public MissingModel GetMissingModel() => default;
            }
            """;
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            text: source,
            path: "SampleController.cs");
        MetadataReference runtimeReference = MetadataReference.CreateFromFile(
            path: typeof(object).Assembly.Location);
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Sample",
            syntaxTrees: [syntaxTree],
            references: [runtimeReference]);

        // When
        Action buildArchitecture = () =>
            ArchitectureAnalysis.Generate(
                compilation: compilation);

        // Then
        buildArchitecture
            .Should()
            .NotThrow();
    }

    [Fact]
    public void BuildShouldHandleMultipleImplementationsOfAnInterface()
    {
        // Given
        const string source = """
            namespace Sample;

            public interface IItem
            {
            }

            public sealed class FirstItem : IItem
            {
            }

            public sealed class SecondItem : IItem
            {
            }

            namespace Services.Foundations
            {
                internal sealed class ItemService(IItem item)
                {
                }
            }
            """;
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            text: source,
            path: "Sample.cs");
        MetadataReference runtimeReference = MetadataReference.CreateFromFile(
            path: typeof(object).Assembly.Location);
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Sample",
            syntaxTrees: [syntaxTree],
            references: [runtimeReference]);

        // When
        Architecture architecture = ArchitectureAnalysis.Generate(
            compilation: compilation);

        // Then
        architecture.Links
            .Should()
            .BeEmpty();
    }

    [Theory]
    [InlineData("\n", "\n", false)]
    [InlineData("\r", "\r", false)]
    [InlineData("\r\n", "\r\n", false)]
    [InlineData("\n", "\r\n", true)]
    [InlineData("\r\n", "\n", true)]
    public void BuildShouldEvaluateLineEndings(
        string firstLineEnding,
        string secondLineEnding,
        bool shouldReportViolation)
    {
        // Given
        string firstSource = string.Join(
            firstLineEnding,
            "namespace Sample.Services.Processings;",
            string.Empty,
            "internal sealed class StudentProcessingService",
            "{",
            "}");
        string secondSource = string.Join(
            secondLineEnding,
            "namespace Sample.Services.Processings;",
            string.Empty,
            "internal sealed class CourseProcessingService",
            "{",
            "}");
        SyntaxTree firstSyntaxTree = CSharpSyntaxTree.ParseText(
            text: firstSource,
            path: "Services/Processings/StudentProcessingService.cs");
        SyntaxTree secondSyntaxTree = CSharpSyntaxTree.ParseText(
            text: secondSource,
            path: "Services/Processings/CourseProcessingService.cs");
        MetadataReference runtimeReference = MetadataReference.CreateFromFile(
            path: typeof(object).Assembly.Location);
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Sample",
            syntaxTrees: [firstSyntaxTree, secondSyntaxTree],
            references: [runtimeReference]);

        // When
        Architecture architecture = ArchitectureAnalysis.Generate(
            compilation: compilation);

        // Then
        architecture.AnalysisItems
            .Any((AnalysisItem item) => item.Code == "STXFORMAT013")
            .Should()
            .Be(shouldReportViolation);
    }
}
