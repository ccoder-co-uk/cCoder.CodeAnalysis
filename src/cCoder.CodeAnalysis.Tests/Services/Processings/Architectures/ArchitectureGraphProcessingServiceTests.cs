// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Architectures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Architectures;

public sealed class ArchitectureGraphProcessingServiceTests
{
    [Fact]
    public void ProcessShouldPropagateExceptionsThroughInterfacesAndPrivateMethods()
    {
        Method controller = CreateMethod(
            id: "StudentController.Put()",
            calls: [CreateCall("IStudentService.Modify()")]);
        Method service = CreateMethod(
            id: "StudentService.Modify()",
            implements: ["IStudentService.Modify()"],
            calls: [CreateCall("StudentService.Validate()")]);
        Method validation = CreateMethod(
            id: "StudentService.Validate()",
            throwsExceptionTypes: ["StudentValidationException"]);
        ArchitectureBuild architectureBuild = CreateBuild(controller, service, validation);
        ArchitectureGraphProcessingService serviceUnderTest = new();

        serviceUnderTest.Process(architectureBuild);

        controller.ThrowsExceptionTypes.Should()
            .ContainSingle()
            .Which.Should()
            .Be("StudentValidationException", "");
    }

    [Fact]
    public void ProcessShouldStopAtDependencyBoundariesAndCycles()
    {
        Method first = CreateMethod(
            id: "First.Run()",
            calls:
            [
                CreateCall("Second.Run()"),
                CreateCall("Database.Save()", isDependencyBoundary: true),
            ]);
        Method second = CreateMethod(
            id: "Second.Run()",
            calls: [CreateCall("First.Run()")],
            throwsExceptionTypes: ["ProcessingException"]);
        ArchitectureBuild architectureBuild = CreateBuild(first, second);
        ArchitectureGraphProcessingService service = new();

        service.Process(architectureBuild);

        first.ThrowsExceptionTypes.Should().Contain("ProcessingException", "");
    }

    [Fact]
    public void ProcessShouldRetainReachableExceptionsAfterTheyAreCaught()
    {
        Method controller = CreateMethod(
            id: "StudentController.Put()",
            calls: [CreateCall("StudentService.Modify()")]);
        controller.ExceptionCatches.Add(
            new ExceptionCatch
            {
                ExceptionType = "StudentValidationException",
                ThrownExceptionTypes = [],
            });
        Method service = CreateMethod(
            id: "StudentService.Modify()",
            throwsExceptionTypes: ["StudentValidationException"]);
        ArchitectureBuild architectureBuild = CreateBuild(controller, service);
        ArchitectureGraphProcessingService serviceUnderTest = new();

        serviceUnderTest.Process(architectureBuild);

        controller.PossibleExceptionTypes.Should().Contain("StudentValidationException", "");
        controller.ThrowsExceptionTypes.Should().NotContain("StudentValidationException", "");
    }

    private static ArchitectureBuild CreateBuild(params Method[] methods) =>

        new()
        {
            Architecture = new Architecture
            {
                Classes =
                [
                    new Class
                    {
                        Name = "Example",
                        StandardElementType = StandardElementType.Unknown,
                        Properties = [],
                        Methods = methods.ToList(),
                        AnalysisMethods = methods.ToList(),
                    },
                ],
                Links = [],
                AnalysisItems = [],
            },
        };

    private static Method CreateMethod(
        string id,
        MethodCall[]? calls = null,
        string[]? implements = null,
        string[]? throwsExceptionTypes = null) =>

        new()
        {
            Id = id,
            Name = id,
            Inputs = [],
            ReturnType = "System.Void",
            Implements = implements?.ToList() ?? [],
            Calls = calls?.ToList() ?? [],
            PossibleExceptionTypes = throwsExceptionTypes?.ToList() ?? [],
            ThrowsExceptionTypes = throwsExceptionTypes?.ToList() ?? [],
            DirectCalls = calls?.ToList() ?? [],
            DirectlyThrowsExceptionTypes = throwsExceptionTypes?.ToList() ?? [],
            ExceptionCatches = [],
        };

    private static MethodCall CreateCall(
        string methodId,
        bool isDependencyBoundary = false) =>

        new()
        {
            TypeName = methodId.Split('.')[0],
            MethodName = methodId,
            MethodId = methodId,
            StandardElementType = isDependencyBoundary
                ? StandardElementType.Dependency
                : StandardElementType.Unknown,
            IsDependencyBoundary = isDependencyBoundary,
        };
}
