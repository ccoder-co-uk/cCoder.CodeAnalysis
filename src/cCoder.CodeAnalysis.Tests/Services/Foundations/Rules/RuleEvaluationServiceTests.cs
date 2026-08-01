// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Brokers.ServiceProviders;
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Foundations.Rules;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;

namespace cCoder.CodeAnalysis.Tests.Services.Foundations.Rules;

public sealed class RuleEvaluationServiceTests
{
    [Fact]
    public void EvaluateShouldResolveStructuralRulesThroughBrokerForInterface()
    {
        AnalysisItem expectedItem = new AnalysisItem { Code = "STXSTRUCT001" };
        Mock<ISTXSTRUCTRulesProcessingService> structuralRulesProcessingServiceMock = new();
        Mock<IServiceProviderBroker> serviceProviderBrokerMock = new();
        EvaluationContext context = CreateContext("public interface IStudentService { }");
        structuralRulesProcessingServiceMock
            .Setup(service => service.Evaluate(context))
            .Returns([expectedItem]);
        serviceProviderBrokerMock
            .Setup(broker => broker.GetStructuralRuleHandlingServices())
            .Returns([structuralRulesProcessingServiceMock.Object]);
        RuleEvaluationService service = new(serviceProviderBrokerMock.Object);

        AnalysisItem[] actualItems = service.Evaluate(context).ToArray();

        actualItems.Should().ContainSingle("").Which.Should().BeSameAs(expectedItem, "");
        serviceProviderBrokerMock.Verify(
            broker => broker.GetStructuralRuleHandlingServices(),
            Times.Once);
        structuralRulesProcessingServiceMock.Verify(
            service => service.Evaluate(context),
            Times.Once);
        serviceProviderBrokerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void EvaluateShouldResolveElementRulesThroughBrokerForClass()
    {
        AnalysisItem expectedItem = new AnalysisItem { Code = "STX0001" };
        Mock<IRuleProcessingService> ruleProcessingServiceMock = new();
        Mock<IServiceProviderBroker> serviceProviderBrokerMock = new();
        EvaluationContext context = CreateContext("public class StudentService { }");
        context.StandardElementType = StandardElementType.FoundationService;
        ruleProcessingServiceMock
            .Setup(service => service.Evaluate(context))
            .Returns([expectedItem]);
        serviceProviderBrokerMock
            .Setup(broker => broker.GetRuleHandlingServices(StandardElementType.FoundationService))
            .Returns([ruleProcessingServiceMock.Object]);
        RuleEvaluationService service = new(serviceProviderBrokerMock.Object);

        AnalysisItem[] actualItems = service.Evaluate(context).ToArray();

        actualItems.Should().ContainSingle("").Which.Should().BeSameAs(expectedItem, "");
        serviceProviderBrokerMock.Verify(
            broker => broker.GetRuleHandlingServices(StandardElementType.FoundationService),
            Times.Once);
        ruleProcessingServiceMock.Verify(
            service => service.Evaluate(context),
            Times.Once);
        serviceProviderBrokerMock.VerifyNoOtherCalls();
    }

    private static EvaluationContext CreateContext(string sourceCode)
    {
        TypeDeclarationSyntax declaration = CSharpSyntaxTree.ParseText(sourceCode)
            .GetCompilationUnitRoot()
            .Members
            .OfType<TypeDeclarationSyntax>()
            .Single();

        return new EvaluationContext
        {
            Declarations = [declaration],
            StandardElementType = StandardElementType.Unknown,
        };
    }
}
