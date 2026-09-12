// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Analyzers;
using cCoder.CodeAnalysis.Services.Foundations.Architectures;
using cCoder.CodeAnalysis.Services.Processings.Architectures;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Architectures;

public sealed class ArchitectureDiagramContractTests
{
    [Fact]
    public void Process_WhenRootConfigurationFactoryComposesConfiguration_ClassifiesItAsApp()
    {
        // Given
        CSharpCompilation compilation = CreateCompilation(
            source:
                """
                namespace Example;

                public static class CoreConfigurationFactory
                {
                    public static object Create() => new object();
                }
                """);

        // When
        Architecture architecture = Process(compilation: compilation);

        // Then
        architecture.Classes.Single()
            .StandardElementType.Should().Be(StandardElementType.App, "configuration composition belongs to the app composition root");
    }

    [Fact]
    public void Process_WhenKnownWebBoundaryTypesUseExternalInfrastructure_ClassifiesThemAsExposures()
    {
        // Given
        CSharpCompilation compilation = CreateCompilation(
            source:
                """
                using System;
                using System.Text;
                namespace Example.Dependencies.Web;

                public sealed class HttpContext { }
                public delegate void RequestDelegate(HttpContext context);

                public sealed class RequestMiddleware : IDisposable
                {
                    public void Dispose() { }
                    public void InvokeAsync(HttpContext context, RequestDelegate next) { }
                }

                public sealed class NotificationHub : IDisposable
                {
                    public void Dispose() { }
                }

                public sealed class ODataModelBuilder
                {
                    private readonly StringBuilder builder = new();
                }
                """);

        // When
        Architecture architecture = Process(compilation: compilation);

        // Then
        architecture.Classes.Single(element => element.Name.EndsWith("RequestMiddleware"))
            .StandardElementType.Should().Be(StandardElementType.HttpExposure, "");
        architecture.Classes.Single(element => element.Name.EndsWith("NotificationHub"))
            .StandardElementType.Should().Be(StandardElementType.Exposure, "");
        architecture.Classes.Single(element => element.Name.EndsWith("ODataModelBuilder"))
            .StandardElementType.Should().Be(StandardElementType.Exposure, "");
    }

    [Fact]
    public void Process_WhenExceptionLivesInModels_ClassifiesItAsAModel()
    {
        // Given
        CSharpCompilation compilation = CreateCompilation(
            source:
                """
                using System;
                namespace Example.Models.Exceptions;
                public sealed class StudentServiceException : Exception { }
                """);

        // When
        Architecture architecture = Process(compilation: compilation);

        // Then
        architecture.Classes.Single()
            .StandardElementType.Should().Be(StandardElementType.Model, "");
    }

    [Fact]
    public void Generate_WhenExceptionIsDiagramModel_DoesNotApplyDataCarrierModelRules()
    {
        // Given
        CSharpCompilation compilation = CreateCompilation(
            source:
                """
                using System;
                namespace Example.Models.Exceptions;
                public sealed class StudentServiceException : InvalidOperationException
                {
                    public StudentServiceException() { }
                    public string Detail { get; set; } = string.Empty;
                }
                """);

        // When
        Architecture architecture = ArchitectureAnalysis.Generate(compilation: compilation);

        // Then
        architecture.Classes.Single()
            .StandardElementType.Should().Be(StandardElementType.Model, "exceptions belong in the model diagram lane");
        architecture.AnalysisItems.Should().NotContain(
            item => item.Code.StartsWith(value: "STXM", comparisonType: StringComparison.Ordinal),
            "exception contracts are not data-carrier models");
    }

    [Fact]
    public void Generate_WhenServiceUsesDomainModelAndException_DoesNotTreatExceptionAsBusinessModelContract()
    {
        // Given
        CSharpCompilation compilation = CreateCompilation(
            source:
                """
                using System;

                namespace Example.Models
                {
                    public sealed class Student { }
                    public sealed class StudentServiceException : Exception { }
                }

                namespace Example.Services.Processings
                {
                    using Example.Models;

                    internal sealed class StudentProcessingService
                    {
                        public void LogStudentFailure(
                            Student student,
                            StudentServiceException exception)
                        {
                        }
                    }
                }
                """);

        // When
        Architecture architecture = ArchitectureAnalysis.Generate(compilation: compilation);

        // Then
        architecture.Classes.Single(element => element.Name.EndsWith("StudentServiceException"))
            .StandardElementType.Should().Be(StandardElementType.Model, "exceptions belong in the model diagram lane");
        architecture.AnalysisItems.Should().NotContain(
            item => item.Type.EndsWith("StudentProcessingService")
                && (item.Code == "STX0007" || item.Code == "STX0018"),
            "exceptions are not business model contracts in a service API");
    }

    [Fact]
    public void Process_WhenClassDependsOnLocalInterface_EmitsLinkToItsConcreteImplementation()
    {
        // Given
        CSharpCompilation compilation = CreateCompilation(
            source:
                """
                namespace Example;

                public interface IStudentBroker { }
                public sealed class StudentBroker : IStudentBroker { }

                public sealed class StudentProcessingService(
                    IStudentBroker studentBroker)
                {
                }
                """);

        // When
        Architecture architecture = Process(compilation: compilation);
        // Then
        architecture.Links.Should().ContainSingle(link =>
            link.FromType.EndsWith("StudentProcessingService", StringComparison.Ordinal)
            && link.ToType.EndsWith("StudentBroker", StringComparison.Ordinal), "");
    }

    [Fact]
    public void Process_WhenClassDependsOnExternalType_EmitsLinkToExternalType()
    {
        // Given
        CSharpCompilation compilation = CreateCompilation(
            source:
                """
                using System;
                namespace Example;

                public sealed class StudentBroker(IDisposable externalDependency)
                {
                }
                """);

        // When
        Architecture architecture = Process(compilation: compilation);

        // Then
        architecture.Links.Should().ContainSingle(link =>
            link.FromType.EndsWith("StudentBroker", StringComparison.Ordinal)
            && link.ToType == "System.IDisposable", "");
    }

    private static Architecture Process(CSharpCompilation compilation)
    {
        ArchitectureBuild build = new() { Compilation = compilation };
        Mock<IArchitectureService> architectureServiceMock = new();
        architectureServiceMock.Setup(service => service.Build(compilation)).Returns(build);
        ArchitectureProcessingService service = new(architectureServiceMock.Object);

        return service.Process(compilation: compilation).Architecture;
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            text: source,
            path: "ArchitectureDiagramContract.cs");

        return CSharpCompilation.Create(
            assemblyName: "Example",
            syntaxTrees: [syntaxTree],
            references:
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Text.StringBuilder).Assembly.Location)
            ]);
    }
}
