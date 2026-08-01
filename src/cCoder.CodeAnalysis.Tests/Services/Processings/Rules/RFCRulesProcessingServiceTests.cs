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
        Method modelMethod = CreateHttpMethod();
        modelMethod.Name = "Post";
        EvaluationContext context = CreateModelContext(modelMethod);

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
    [InlineData("RFC0011")]
    [InlineData("RFC0012")]
    [InlineData("RFC0013")]
    [InlineData("RFC0014")]
    [InlineData("RFC0015")]
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
            case "RFC0011":
                method.HttpResponses.Add(new HttpResponse
                {
                    StatusCode = 204,
                    ResultMethod = "StatusCode",
                    HasBody = true,
                });
                break;
            case "RFC0012":
                method.HttpMethods.Add("HEAD");
                method.HttpResponses.Add(new HttpResponse
                {
                    StatusCode = 200,
                    ResultMethod = "Ok",
                    HasBody = true,
                });
                break;
            case "RFC0013":
                method.ThrowsExceptionTypes.Add("UnsupportedMediaException");
                break;
            case "RFC0014":
                method.ThrowsExceptionTypes.Add("PreconditionException");
                break;
            case "RFC0015":
                method.HttpResponses.Add(new HttpResponse
                {
                    StatusCode = 799,
                    ResultMethod = "StatusCode",
                });
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
        method.HttpResponses.Add(new HttpResponse
        {
            ResultMethod = "Ok",
            StatusCode = 200,
        });
        method.ThrowsExceptionTypes.AddRange(
            [
                "AppValidationException",
                "AppAuthenticationException",
                "AppAuthorizationException",
                "AppConcurrencyException",
                "UnsupportedMediaException",
                "PreconditionException",
            ]);
        method.HttpResponses.AddRange(
            [
                CreateExceptionResponse("AppValidationException", 400, "BadRequest"),
                CreateExceptionResponse("AppAuthenticationException", 401, "Challenge"),
                CreateExceptionResponse("AppAuthorizationException", 403, "Forbid"),
                CreateExceptionResponse("AppConcurrencyException", 409, "Conflict"),
                CreateExceptionResponse("UnsupportedMediaException", 415, "UnsupportedMediaType"),
                CreateExceptionResponse("PreconditionException", 412, "PreconditionFailed"),
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
        MethodDeclarationSyntax declarationMethod = declaration.Members
            .OfType<MethodDeclarationSyntax>()
            .Single();
        string methodName = declarationMethod.Identifier.Text;
        string httpMethod = methodName switch
        {
            "Get" or "GetAll" => "GET",
            "Post" => "POST",
            "Put" => "PUT",
            "Patch" => "PATCH",
            "Delete" => "DELETE",
            _ => string.Empty,
        };
        Dictionary<string, int> statusCodes = new(StringComparer.Ordinal)
        {
            ["Ok"] = 200,
            ["Updated"] = 200,
            ["Created"] = 201,
            ["CreatedAtAction"] = 201,
            ["CreatedAtRoute"] = 201,
            ["NoContent"] = 204,
        };
        List<HttpResponse> responses = declarationMethod.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Select(invocation => invocation.Expression switch
            {
                IdentifierNameSyntax identifier => identifier.Identifier.Text,
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
                _ => string.Empty,
            })
            .Where(statusCodes.ContainsKey)
            .Select(resultMethod => new HttpResponse
            {
                ResultMethod = resultMethod,
                StatusCode = statusCodes[resultMethod],
            })
            .ToList();
        Method modelMethod = CreateHttpMethod();
        modelMethod.Name = methodName;
        modelMethod.LineNumber = declarationMethod.GetLocation()
            .GetLineSpan().StartLinePosition.Line + 1;
        modelMethod.IsODataControllerAction = true;
        modelMethod.HasFromBodyParameter = declarationMethod.ParameterList.Parameters.Any(
            parameter => parameter.AttributeLists
                .SelectMany(attributes => attributes.Attributes)
                .Any(attribute => attribute.Name.ToString() == "FromBody"));
        modelMethod.HttpResponses = responses;

        if (httpMethod.Length > 0)
        {
            modelMethod.HttpMethods.Add(httpMethod);
        }

        return CreateModelContext(method: modelMethod);
    }

    private static EvaluationContext CreateModelContext(Method method)
    {
        Class element = new()
        {
            Name = "Example.AppController",
            StandardElementType = StandardElementType.HttpExposure,
            LineNumber = 1,
            Properties = [],
            Methods = [method],
            AnalysisMethods = [method],
        };

        return new EvaluationContext
        {
            ArchitectureElement = element,
            ArchitectureModel = new Architecture
            {
                Project = new ProjectMetadata { AssemblyName = "Example" },
                Classes = [element],
            },
        };
    }

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