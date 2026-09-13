// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class ServiceContractRuleTests
{
    [Fact]
    public void Service_WhenImplementingSharedLocalContract_IsNotReportedForContractName()
    {
        const string contractName = "Example.Rules.IRule";

        Class firstRule = CreateService(
            name: "Example.Rules.FirstRuleProcessingService",
            contractName: contractName);

        Class secondRule = CreateService(
            name: "Example.Rules.SecondRuleProcessingService",
            contractName: contractName);

        Class sharedContract = new()
        {
            Name = contractName,
            Kind = ArchitectureTypeKind.Interface,
            IsPublic = false,
            StandardElementType = StandardElementType.ProcessingService,
        };

        EvaluationContext context = new()
        {
            ArchitectureElement = firstRule,
            ArchitectureModel = new Architecture
            {
                Classes = [firstRule, secondRule],
                Interfaces = [sharedContract],
            },
        };

        new STXRulesProcessingService()
            .Evaluate(context: context)
            .Should()
            .NotContain(item => item.Code == "STX0014");
    }

    [Fact]
    public void Service_WhenImplementingUnsharedMismatchedContract_IsReportedForContractName()
    {
        Class service = CreateService(
            name: "Example.Rules.FirstRuleProcessingService",
            contractName: "Example.Rules.IRule");

        EvaluationContext context = new()
        {
            ArchitectureElement = service,
            ArchitectureModel = new Architecture
            {
                Classes = [service],
                Interfaces =
                [
                    new Class
                    {
                        Name = "Example.Rules.IRule",
                        Kind = ArchitectureTypeKind.Interface,
                        IsPublic = false,
                        StandardElementType = StandardElementType.ProcessingService,
                    },
                ],
            },
        };

        new STXRulesProcessingService()
            .Evaluate(context: context)
            .Should()
            .ContainSingle(item => item.Code == "STX0014");
    }

    private static Class CreateService(string name, string contractName) =>
        new()
        {
            Name = name,
            StandardElementType = StandardElementType.ProcessingService,
            AnalysisDeclarations = [],
            AnalysisDependencies = [],
            AnalysisImplementedInterfaces = [contractName],
            AnalysisPublicApiModelTypes = [],
            AnalysisContractMethodNames = [],
        };
}