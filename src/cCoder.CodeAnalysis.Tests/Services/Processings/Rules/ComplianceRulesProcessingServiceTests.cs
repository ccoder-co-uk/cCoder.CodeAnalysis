// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class ComplianceRulesProcessingServiceTests
{
    [Theory]
    [InlineData("POST", false, false, "ODATA0001")]
    [InlineData("GET", true, false, "ODATA0002")]
    public void ODataRulesShouldRequireCreateAndNotFoundResponses(
        string httpMethod,
        bool hasKeyParameter,
        bool handlesNullWithNotFound,
        string expectedCode)
    {
        Method method = CreateMethod();
        method.HttpMethods.Add(httpMethod);
        method.HasKeyParameter = hasKeyParameter;
        method.HandlesNullWithNotFound = handlesNullWithNotFound;
        EvaluationContext context = CreateContext(method: method);
        ODATARulesProcessingService service = new();

        service.Evaluate(context: context)
            .Should()
            .ContainSingle(item => item.Code == expectedCode);
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

    private static EvaluationContext CreateContext(Method method) =>

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
