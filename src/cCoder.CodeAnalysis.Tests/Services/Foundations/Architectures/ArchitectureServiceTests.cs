// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Foundations.Architectures;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace cCoder.CodeAnalysis.Tests.Services.Foundations.Architectures;

public sealed class ArchitectureServiceTests
{
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
        ArchitectureService architectureService = new ArchitectureService();

        // When
        Architecture architecture = architectureService.Build(
            compilation: compilation);

        // Then
        architecture.Links
            .Should()
            .BeEmpty();
    }
}