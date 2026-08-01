// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class STXDRulesProcessingServiceTests
{
    [Fact]
    public void EvaluateShouldRejectExternalResourceInBroker()
    {
        EvaluationContext context = new()
        {
            TypeName = "Example.Brokers.MailBroker",
            StandardElementType = StandardElementType.Broker,
            Declarations = [],
            Dependencies = [],
            LocalDependencyTypeNames = [],
            ImplementedInterfaces = ["Example.Brokers.IMailBroker"],
            UsesExternalResource = true
        };

        STXDRulesProcessingService service = new();

        service.Evaluate(context: context)
            .Should()
            .ContainSingle(
                predicate: item => item.Code == "STXD004");
    }

    [Fact]
    public void EvaluateShouldRejectExternalResourceInHttpExposure()
    {
        EvaluationContext context = new()
        {
            TypeName = "Example.Controllers.StudentController",
            StandardElementType = StandardElementType.HttpExposure,
            Declarations = [],
            Dependencies = [],
            LocalDependencyTypeNames = [],
            ImplementedInterfaces = [],
            UsesExternalResource = true
        };

        new STXDRulesProcessingService().Evaluate(context)
            .Should().ContainSingle(item => item.Code == "STXD004");
    }
}
