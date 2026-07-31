// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class RFCRulesProcessingServiceTests
{
    private readonly RFCRulesProcessingService service = new();

    [Theory]
    [InlineData(
        "public IActionResult Post([FromBody] App app) => Ok(app);",
        "RFC0001")]
    [InlineData(
        "public IActionResult Delete(int key) => Ok();",
        "RFC0002")]
    [InlineData(
        "public IActionResult Get() => NoContent();",
        "RFC0003")]
    [InlineData(
        "public IActionResult Put(int key, App app) => NoContent();",
        "RFC0004")]
    [InlineData(
        "public IActionResult Patch(int key, App app) => NoContent();",
        "RFC0004")]
    public void EvaluateShouldRejectNonCompliantCrudResult(
        string method,
        string expectedCode)
    {
        EvaluationContext context = CreateContext(method: method);

        service.Evaluate(context: context)
            .Should()
            .ContainSingle(item => item.Code == expectedCode);
    }

    [Theory]
    [InlineData(
        "public IActionResult Post([FromBody] App app) => Created(app);")]
    [InlineData(
        "public IActionResult Delete(int key) => NoContent();")]
    [InlineData(
        "public IActionResult Get() => Ok(new App());")]
    [InlineData(
        "public IActionResult Put(int key, App app) => Ok(app);")]
    [InlineData(
        "public IActionResult Patch(int key, App app) => Updated(app);")]
    public void EvaluateShouldAllowCompliantCrudResult(string method)
    {
        EvaluationContext context = CreateContext(method: method);

        service.Evaluate(context: context)
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void EvaluateShouldAllowActionStylePost()
    {
        EvaluationContext context = CreateContext(
            method:
                "public IActionResult PostUpdateOrder(App app) => Ok();");

        service.Evaluate(context: context)
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void EvaluateShouldIgnoreNonODataController()
    {
        TypeDeclarationSyntax declaration = ParseDeclaration(
            """
            public class AppController : Controller
            {
                public IActionResult Post([FromBody] App app) => Ok(app);
            }
            """);

        EvaluationContext context = new()
        {
            TypeName = "Example.AppController",
            Declarations = [declaration],
        };

        service.Evaluate(context: context)
            .Should()
            .BeEmpty();
    }

    [Theory]
    [InlineData("RFC0005")]
    [InlineData("RFC0006")]
    [InlineData("RFC0007")]
    [InlineData("RFC0008")]
    [InlineData("RFC0009")]
    [InlineData("RFC0010")]
    public void EvaluateShouldRejectMissingHttpFailureMapping(string expectedCode)
    {
        Method method = CreateHttpMethod();

        switch (expectedCode)
        {
            case "RFC0005":
                method.ThrowsExceptionTypes.Add("AppValidationException");
                break;
            case "RFC0006":
                method.ThrowsExceptionTypes.Add("AppAuthenticationException");
                method.HttpResponses.Add(CreateExceptionResponse("AppAuthenticationException", 401, "Unauthorized"));
                break;
            case "RFC0007":
                method.ThrowsExceptionTypes.Add("AppAuthorizationException");
                method.HttpResponses.Add(CreateExceptionResponse("AppAuthorizationException", 401, "Unauthorized"));
                break;
            case "RFC0008":
                method.IsODataControllerAction = true;
                method.HttpMethods.Add("GET");
                method.HasKeyParameter = true;
                break;
            case "RFC0009":
                method.ThrowsExceptionTypes.Add("AppConcurrencyException");
                break;
            case "RFC0010":
                method.HttpResponses.Add(CreateExceptionResponse("System.Exception", 400, "BadRequest"));
                break;
        }

        EvaluationContext context = CreateModelContext(method: method);

        service.Evaluate(context: context)
            .Should()
            .ContainSingle(item => item.Code == expectedCode);
    }

    [Fact]
    public void EvaluateShouldAllowCompliantHttpFailureMappings()
    {
        Method method = CreateHttpMethod();
        method.IsODataControllerAction = true;
        method.HttpMethods.Add("GET");
        method.HasKeyParameter = true;
        method.HandlesNullWithNotFound = true;
        method.ThrowsExceptionTypes.AddRange(
            [
                "AppValidationException",
                "AppAuthenticationException",
                "AppAuthorizationException",
                "AppConcurrencyException",
            ]);
        method.HttpResponses.AddRange(
            [
                CreateExceptionResponse("AppValidationException", 400, "BadRequest"),
                CreateExceptionResponse("AppAuthenticationException", 401, "Challenge"),
                CreateExceptionResponse("AppAuthorizationException", 403, "Forbid"),
                CreateExceptionResponse("AppConcurrencyException", 409, "Conflict"),
            ]);
        EvaluationContext context = CreateModelContext(method: method);

        service.Evaluate(context: context)
            .Should()
            .BeEmpty();
    }

    private static EvaluationContext CreateContext(string method)
    {
        TypeDeclarationSyntax declaration = ParseDeclaration(
            $$"""
            public class AppController : ODataController
            {
                {{method}}
            }
            """);

        return new EvaluationContext
        {
            TypeName = "Example.AppController",
            Declarations = [declaration],
        };
    }

    private static EvaluationContext CreateModelContext(Method method) =>

        new()
        {
            TypeName = "Example.AppController",
            Declarations = [],
            ArchitectureElement = new Class
            {
                Name = "Example.AppController",
                StandardElementType = StandardElementType.Exposure,
                Properties = [],
                Methods = [method],
                AnalysisMethods = [method],
            },
        };

    private static Method CreateHttpMethod() =>

        new()
        {
            Id = "Example.AppController.Get(System.Int32)",
            Name = "Get",
            LineNumber = 10,
            Inputs = [],
            ReturnType = "IActionResult",
            Implements = [],
            Calls = [],
            ThrowsExceptionTypes = [],
            HttpMethods = [],
            HttpResponses = [],
            IsHttpRequestHandler = true,
            DirectCalls = [],
            DirectlyThrowsExceptionTypes = [],
            ExceptionCatches = [],
        };

    private static HttpResponse CreateExceptionResponse(
        string exceptionType,
        int statusCode,
        string resultMethod) =>

        new()
        {
            ExceptionType = exceptionType,
            StatusCode = statusCode,
            ResultMethod = resultMethod,
            IsExceptionPath = true,
        };

    private static TypeDeclarationSyntax ParseDeclaration(string source) =>
        CSharpSyntaxTree.ParseText(text: source)
            .GetRoot()
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Single();
}
