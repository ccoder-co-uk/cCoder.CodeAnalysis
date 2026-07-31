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
    public void ProcessShouldCaptureHttpResponseAndExceptionPaths()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            text:
                """
                using System;
                namespace Example.Controllers;

                public sealed class HttpGetAttribute : Attribute { }
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
        controller.StandardElementType.Should().Be(StandardElementType.Exposure, "");
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
