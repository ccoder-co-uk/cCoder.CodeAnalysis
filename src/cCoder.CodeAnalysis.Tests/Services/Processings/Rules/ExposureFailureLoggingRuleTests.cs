// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class ExposureFailureLoggingRuleTests
{
    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void RethrownExposureFailureShouldRequireLogging(
        bool logsException,
        bool expectsDiagnostic)
    {
        Method method = CreateMethod();
        method.ExceptionCatches =
        [
            new ExceptionCatch
            {
                ExceptionType = "System.Exception",
                ThrownExceptionTypes = [],
                Rethrows = true,
                LogsException = logsException,
            }
        ];

        AnalysisItem[] results = new STXERulesProcessingService()
            .Evaluate(context: CreateContext(StandardElementType.Exposure, method))
            .ToArray();

        results.Any(result => result.Code == "STXE008")
            .Should().Be(expectsDiagnostic);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void HttpFailureResponseShouldRequireLogging(
        bool logsException,
        bool expectsDiagnostic)
    {
        Method method = CreateMethod();
        method.IsHttpRequestHandler = true;
        method.HttpResponses =
        [
            new HttpResponse
            {
                StatusCode = 500,
                IsExceptionPath = true,
                LogsException = logsException,
            }
        ];

        EvaluationContext context = CreateContext(
            StandardElementType.HttpExposure,
            method);

        context.ArchitectureElement.AnalysisIsApiController = true;

        AnalysisItem[] results = new STXAPIRulesProcessingService()
            .Evaluate(context: context)
            .ToArray();

        results.Any(result => result.Code == "STXAPI006")
            .Should().Be(expectsDiagnostic);
    }

    private static Method CreateMethod() =>
        new()
        {
            Id = "Example.Exposure.Execute()",
            Name = "Execute",
            LineNumber = 10,
            Inputs = [],
            Implements = [],
            Calls = [],
            PossibleExceptionTypes = [],
            IncomingExceptionTypes = [],
            ThrowsExceptionTypes = [],
            HttpMethods = [],
            HttpResponses = [],
            DirectCalls = [],
            DirectlyThrowsExceptionTypes = [],
            ExceptionCatches = [],
        };

    private static EvaluationContext CreateContext(
        StandardElementType elementType,
        Method method)
    {
        Class element = new()
        {
            Name = "Example.Exposure",
            StandardElementType = elementType,
            Methods = [method],
            AnalysisMethods = [method],
            AnalysisDependencies = [],
            AnalysisImplementedInterfaces = [],
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
}
