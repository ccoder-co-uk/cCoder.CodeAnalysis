// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Foundations.Architectures;
using cCoder.CodeAnalysis.Services.Processings.Architectures;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Architectures;

public sealed partial class ArchitectureProcessingServiceTests
{
    [Fact]
    public void Process_WhenCompilationIsTestAssembly_DoesNotBuildUnusedCallGraph()
    {
        // Given
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            text:
                """
                namespace Example.Tests;

                public sealed class StudentTests
                {
                    public void Execute() => System.Console.WriteLine("test");
                }
                """,
            path: "StudentTests.cs");
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Example.Tests",
            syntaxTrees: [syntaxTree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        ArchitectureBuild build = new() { Compilation = compilation };
        Mock<IArchitectureService> architectureServiceMock = new();
        architectureServiceMock.Setup(service => service.Build(compilation)).Returns(build);
        ArchitectureProcessingService service = new(architectureServiceMock.Object);

        // When
        ArchitectureBuild result = service.Process(compilation);

        // Then
        result.Architecture.Classes
            .Single(element => element.Name == "Example.Tests.StudentTests")
            .Methods.Single(method => method.Name == "Execute")
            .DirectCalls.Should().BeEmpty();
    }
}
