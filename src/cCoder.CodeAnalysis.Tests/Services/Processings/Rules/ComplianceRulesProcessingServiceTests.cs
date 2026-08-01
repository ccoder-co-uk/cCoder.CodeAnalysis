// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class ComplianceRulesProcessingServiceTests
{
    [Fact]
    public void ODataRuleShouldRequireCreatedResponseForEntityCreate()
    {
        Method method = CreateMethod();
        method.Name = "Post";
        method.HttpMethods.Add("POST");
        method.HasFromBodyParameter = true;
        EvaluationContext context = CreateContext(method: method);
        ODATARulesProcessingService service = new();

        service.Evaluate(context: context)
            .Should()
            .ContainSingle(item => item.Code == "ODATA0001");
    }

    [Theory]
    [InlineData("PostCopyAsync", "POST", true)]
    [InlineData("GetRender", "POST", false)]
    [InlineData("DeleteAll", "POST", true)]
    public void ODataRuleShouldAllowNonCreationPostActions(
        string methodName,
        string httpMethod,
        bool hasFromBodyParameter)
    {
        Method method = CreateMethod();
        method.Name = methodName;
        method.HttpMethods.Add(httpMethod);
        method.HasFromBodyParameter = hasFromBodyParameter;
        EvaluationContext context = CreateContext(method: method);
        ODATARulesProcessingService service = new();

        service.Evaluate(context: context)
            .Should()
            .NotContain(item => item.Code == "ODATA0001");
    }

    [Fact]
    public void ODataRuleShouldRequireNotFoundForKeyedEntityRead()
    {
        Method method = CreateMethod();
        method.Name = "Get";
        method.HttpMethods.Add("GET");
        method.HasKeyParameter = true;
        EvaluationContext context = CreateContext(method: method);
        ODATARulesProcessingService service = new();

        service.Evaluate(context: context)
            .Should()
            .ContainSingle(item => item.Code == "ODATA0002");
    }

    [Theory]
    [InlineData("GetAll")]
    [InlineData("GetRender")]
    [InlineData("GetRootFor")]
    public void ODataRuleShouldAllowNonEntityGetActions(string methodName)
    {
        Method method = CreateMethod();
        method.Name = methodName;
        method.HttpMethods.Add("GET");
        method.HasKeyParameter = false;
        EvaluationContext context = CreateContext(method: method);
        ODATARulesProcessingService service = new();

        service.Evaluate(context: context)
            .Should()
            .NotContain(item => item.Code == "ODATA0002");
    }

    [Fact]
    public void ODataRuleShouldRequireNotImplementedResponse()
    {
        Method method = CreateMethod();
        method.ThrowsExceptionTypes.Add("System.NotImplementedException");
        EvaluationContext context = CreateContext(method: method);
        ODATARulesProcessingService service = new();

        service.Evaluate(context: context)
            .Should()
            .ContainSingle(item => item.Code == "ODATA0003");
    }

    [Fact]
    public void OwaspRuleShouldRejectExceptionDetailDisclosure()
    {
        Method method = CreateMethod();
        method.HttpResponses.Add(
            new HttpResponse
            {
                StatusCode = 500,
                ResultMethod = "StatusCode",
                ExceptionType = "System.Exception",
                IsExceptionPath = true,
                ExposesExceptionDetails = true,
            });
        EvaluationContext context = CreateContext(method: method);
        OWASPRulesProcessingService service = new();

        service.Evaluate(context: context)
            .Should()
            .ContainSingle(item => item.Code == "OWASP0001");
    }

    private static EvaluationContext CreateContext(Method method)
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

    private static Method CreateMethod() =>

        new()
        {
            Id = "Example.AppController.Action()",
            Name = "Action",
            LineNumber = 10,
            Inputs = [],
            ReturnType = "IActionResult",
            Implements = [],
            Calls = [],
            ThrowsExceptionTypes = [],
            HttpMethods = [],
            HttpResponses = [],
            IsHttpRequestHandler = true,
            IsODataControllerAction = true,
            DirectCalls = [],
            DirectlyThrowsExceptionTypes = [],
            ExceptionCatches = [],
        };
}