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

public sealed class ArchitectureProcessingServiceTests
{
    [Fact]
    public void ProcessShouldPopulateBrowserSafeProjectAndTypeSchema()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            text:
                """
                using System;
                namespace Example;

                internal interface IMarker { }
                public interface IStudent : IMarker { }
                public class StudentBase { }

                public sealed class Student : StudentBase, IStudent, IDisposable
                {
                    public void Dispose() { }
                }

                public sealed class StudentException : Exception { }
                """,
            path: "C:\\private\\Students.cs");
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Example",
            syntaxTrees: [syntaxTree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        ArchitectureBuild build = new() { Compilation = compilation };
        Mock<IArchitectureService> architectureServiceMock = new();
        architectureServiceMock.Setup(service => service.Build(compilation)).Returns(build);
        ArchitectureProcessingService service = new(architectureServiceMock.Object);

        ArchitectureBuild result = service.Process(compilation);

        result.Architecture.Project.Id.Should().Be("Example", "");
        result.Architecture.Project.Name.Should().Be("Example", "");
        result.Architecture.Project.AssemblyName.Should().Be("Example", "");
        result.Architecture.Interfaces.Should().Contain(
            element => element.Name == "Example.IStudent"
                && element.Kind == ArchitectureTypeKind.Interface,
            "");
        Class student = result.Architecture.Classes.Single(
            element => element.Name == "Example.Student");
        student.Kind.Should().Be(ArchitectureTypeKind.Class, "");
        student.IsPublic.Should().BeTrue("");
        student.LineNumber.Should().BeGreaterThan(0, "");
        student.BaseType.Should().NotBeNull("");
        student.BaseType.Id.Should().Be("Example:Example.StudentBase", "");
        student.BaseType.IsInCurrentProject.Should().BeTrue("");
        student.Interfaces.Select(reference => reference.Id).Should().Equal(
            "Example:Example.IStudent",
            "System.Private.CoreLib:System.IDisposable");
        TypeReference localContract = student.Interfaces.Single(
            reference => reference.Name == "IStudent");
        localContract.FullName.Should().Be("Example.IStudent", "");
        localContract.Namespace.Should().Be("Example", "");
        localContract.AssemblyName.Should().Be("Example", "");
        localContract.Kind.Should().Be(ArchitectureTypeKind.Interface, "");
        localContract.IsInCurrentProject.Should().BeTrue("");
        localContract.StandardElementType.Should().Be(
            result.Architecture.Interfaces.Single(element => element.Name == "Example.IStudent")
                .StandardElementType,
            "");
        student.Interfaces.Single(reference => reference.Name == "IDisposable")
            .StandardElementType.Should().Be(StandardElementType.Dependency, "");
        student.AnalysisImplementedInterfaces.Should().Contain(
            ["Example.IMarker", "Example.IStudent", "System.IDisposable"],
            "");
        Class exception = result.Architecture.Classes.Single(
            element => element.Name == "Example.StudentException");
        exception.BaseType.Id.Should().Be(
            "System.Private.CoreLib:System.Exception",
            "");
        exception.BaseType.IsInCurrentProject.Should().BeFalse("");

        string json = ArchitectureJsonSerializer.Serialize(result.Architecture);
        json.Should().NotContain("C:\\private", "");
        json.Should().NotContain("AnalysisImplementedInterfaces", "");
    }

    [Fact]
    public void ProcessShouldClassifyOnlyHttpRequestHandlersAsHttpExposures()
    {
        // Given
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            text:
                """
                namespace Example;

                public sealed class HttpContext { }
                public delegate void RequestDelegate(HttpContext context);
                public interface IMiddleware { }

                public sealed class ConventionalMiddleware
                {
                    public object InvokeAsync(
                        HttpContext context,
                        RequestDelegate next) => new object();

                    public object Handle() => new object();
                }

                public sealed class InterfaceMiddleware : IMiddleware
                {
                    public object InvokeAsync(HttpContext context) => new object();
                }

                public sealed class UnrelatedHandler
                {
                    public object InvokeAsync(string value) => value;
                }

                namespace Exposures
                {
                    public sealed class EventExposure
                    {
                        public object Handle() => new object();
                    }
                }
                """,
            path: "Middleware.cs");
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Example",
            syntaxTrees: [syntaxTree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        ArchitectureBuild build = new() { Compilation = compilation };
        Mock<IArchitectureService> architectureServiceMock = new();
        architectureServiceMock.Setup(service => service.Build(compilation)).Returns(build);
        ArchitectureProcessingService service = new(architectureServiceMock.Object);

        // When
        Architecture architecture = service.Process(compilation).Architecture;

        // Then
        Class conventional = architecture.Classes.Single(element =>
            element.Name == "Example.ConventionalMiddleware");
        conventional.StandardElementType.Should().Be(StandardElementType.HttpExposure, "");
        conventional.Methods.Single(method => method.Name == "InvokeAsync")
            .IsHttpRequestHandler.Should().BeTrue("");
        conventional.Methods.Single(method => method.Name == "Handle")
            .IsHttpRequestHandler.Should().BeFalse("");

        architecture.Classes.Single(element => element.Name == "Example.InterfaceMiddleware")
            .StandardElementType.Should().Be(StandardElementType.HttpExposure, "");
        Class unrelated = architecture.Classes.Single(element =>
            element.Name == "Example.UnrelatedHandler");
        unrelated.StandardElementType.Should().Be(StandardElementType.Unknown, "");
        unrelated.Methods.Single(method => method.Name == "InvokeAsync")
            .IsHttpRequestHandler.Should().BeFalse("");
        architecture.Classes.Single(element => element.Name == "Example.Exposures.EventExposure")
            .StandardElementType.Should().Be(StandardElementType.Exposure, "");
    }

    [Fact]
    public void ProcessShouldCaptureHttpResponseAndExceptionPaths()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            text:
                """
                using System;
                namespace Example.Controllers;

                public sealed class HttpGetAttribute : Attribute { }
                public sealed class HttpHeadAttribute : Attribute { }
                public class ODataController
                {
                    protected object Ok(object value) => value;
                    protected object BadRequest() => new object();
                    protected object Forbid() => new object();
                    protected object NotFound() => new object();
                }
                public sealed class StudentValidationException : Exception { }
                public sealed class StudentAuthorizationException : Exception { }

                public sealed class StudentController : ODataController
                {
                    [HttpHead]
                    public object GetStudentHeaders() => Ok(new object());

                    [HttpGet]
                    public object GetStudent(int studentId)
                    {
                        try
                        {
                            object? student = null;

                            if (student is null)
                            {
                                return NotFound();
                            }

                            return Ok(student);
                        }
                        catch (StudentValidationException)
                        {
                            return BadRequest();
                        }
                        catch (StudentAuthorizationException)
                        {
                            return Forbid();
                        }
                    }
                }
                """,
            path: "StudentController.cs");
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Example",
            syntaxTrees: [syntaxTree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        ArchitectureBuild build = new() { Compilation = compilation };
        Mock<IArchitectureService> architectureServiceMock = new();
        architectureServiceMock.Setup(service => service.Build(compilation)).Returns(build);
        ArchitectureProcessingService service = new(architectureServiceMock.Object);

        ArchitectureBuild result = service.Process(compilation);

        Method action = result.Architecture.Classes
            .Single(element => element.Name == "Example.Controllers.StudentController")
            .Methods.Single(method => method.Name == "GetStudent");
        action.IsHttpRequestHandler.Should().BeTrue("");
        action.IsODataControllerAction.Should().BeTrue("");
        action.HasKeyParameter.Should().BeTrue("");
        action.HandlesNullWithNotFound.Should().BeTrue("");
        action.HttpMethods.Should().ContainSingle().Which.Should().Be("GET", "");
        action.HttpResponses.Should().Contain(
            response => response.StatusCode == 200 && !response.IsExceptionPath,
            "");
        action.HttpResponses.Should().Contain(
            response => response.StatusCode == 404 && response.IsNullPath,
            "");
        action.HttpResponses.Should().Contain(
            response => response.StatusCode == 400
                && response.ExceptionType == "Example.Controllers.StudentValidationException",
            "");
        action.HttpResponses.Should().Contain(
            response => response.StatusCode == 403
                && response.ExceptionType == "Example.Controllers.StudentAuthorizationException",
            "");
        Method headAction = result.Architecture.Classes
            .Single(element => element.Name == "Example.Controllers.StudentController")
            .Methods.Single(method => method.Name == "GetStudentHeaders");
        headAction.HttpMethods.Should().ContainSingle().Which.Should().Be("HEAD", "");
        headAction.HttpResponses.Should().ContainSingle().Which.HasBody.Should().BeTrue("");
    }

    [Fact]
    public void ProcessShouldCaptureMethodCallsExceptionsAndDependencyBoundaries()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            text:
                """
                using System;
                namespace Example.Controllers;

                public interface IStudentBroker
                {
                    void SelectStudent();
                }

                public sealed class StudentController(IStudentBroker studentBroker)
                {
                    public void GetStudent(string studentId)
                    {
                        ValidateStudent();
                        studentBroker.SelectStudent();
                        studentId.Trim();
                        throw new StudentValidationException();
                    }

                    private static void ValidateStudent()
                    {
                    }
                }

                public sealed class StudentValidationException : Exception
                {
                }
                """,
            path: "StudentController.cs");
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Example",
            syntaxTrees: [syntaxTree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        ArchitectureBuild build = new()
        {
            Compilation = compilation,
        };
        Mock<IArchitectureService> architectureServiceMock = new();
        architectureServiceMock
            .Setup(service => service.Build(compilation))
            .Returns(build);
        ArchitectureProcessingService service =
            new(architectureServiceMock.Object);

        ArchitectureBuild result = service.Process(compilation);

        Class controller = result.Architecture.Classes.Single(
            element => element.Name == "Example.Controllers.StudentController");
        controller.StandardElementType.Should().Be(StandardElementType.HttpExposure, "");
        Method getStudent = controller.Methods.Single(method => method.Name == "GetStudent");
        getStudent.Id.Should().Be("Example.Controllers.StudentController.GetStudent(System.String)", "");
        getStudent.ThrowsExceptionTypes.Should()
            .ContainSingle()
            .Which.Should()
            .Be("Example.Controllers.StudentValidationException", "");
        getStudent.Calls.Should().Contain(
            call => call.MethodId == "Example.Controllers.IStudentBroker.SelectStudent()"
                && !call.IsDependencyBoundary,
            "");
        getStudent.Calls.Should().Contain(
            call => call.MethodId == "System.String.Trim()"
                && call.StandardElementType == StandardElementType.Dependency
                && call.IsDependencyBoundary,
            "");
        getStudent.Calls.Should().NotContain(call => call.MethodName == "ValidateStudent", "");
        getStudent.DirectCalls.Should().Contain(call => call.MethodName == "ValidateStudent", "");

        string json = ArchitectureJsonSerializer.Serialize(result.Architecture);
        json.Should().NotContain("AnalysisMethods", "");
        json.Should().NotContain("DirectCalls", "");
        json.Should().NotContain("TargetSymbol", "");
    }

    [Fact]
    public void ProcessShouldCaptureExpressionBodiedThrowException()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            text:
                """
                using System;

                public sealed class StudentValidationException : Exception
                {
                }

                public sealed class StudentController
                {
                    public object PostStudent() =>
                        throw new StudentValidationException();
                }
                """,
            path: "StudentController.cs");
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Example",
            syntaxTrees: [syntaxTree],
            references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        ArchitectureBuild build = new() { Compilation = compilation };
        Mock<IArchitectureService> architectureServiceMock = new();
        architectureServiceMock.Setup(service => service.Build(compilation)).Returns(build);
        ArchitectureProcessingService service = new(architectureServiceMock.Object);

        ArchitectureBuild result = service.Process(compilation);

        Method method = result.Architecture.Classes
            .Single(element => element.Name == "StudentController")
            .Methods.Single(item => item.Name == "PostStudent");
        method.ThrowsExceptionTypes.Should()
            .ContainSingle()
            .Which.Should()
            .Be("StudentValidationException", "");
    }

    [Fact]
    public void ProcessShouldExcludeGeneratedSyntaxTrees()
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Example",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    text: "public sealed class Student { }",
                    path: "Student.cs"),
                CSharpSyntaxTree.ParseText(
                    text:
                        """
                        // <auto-generated/>
                        public sealed class GeneratedPage
                        {
                            public void Render()
                            {
                                if (true)
                                {
                                }
                            }
                        }
                        """,
                    path: "obj/GeneratedPage.g.cs"),
                CSharpSyntaxTree.ParseText(
                    text:
                        """
                        //------------------------------------------------------------------------------
                        // <auto-generated>
                        //     This code was generated by a tool.
                        // </auto-generated>
                        public sealed class GeneratedOpenApiSupport { }
                        """,
                    path: "OpenApiXmlCommentSupport.generated.cs"),
            ]);
        ArchitectureBuild build = new()
        {
            Compilation = compilation,
        };
        Mock<IArchitectureService> architectureServiceMock = new();
        architectureServiceMock
            .Setup(service => service.Build(compilation))
            .Returns(build);
        ArchitectureProcessingService service =
            new(architectureServiceMock.Object);

        ArchitectureBuild result = service.Process(compilation);

        result.Architecture.Classes
            .Select(item => item.Name)
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be("Student");
    }
}