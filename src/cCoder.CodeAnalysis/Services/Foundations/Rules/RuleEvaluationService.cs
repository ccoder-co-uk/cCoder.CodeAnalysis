// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Brokers.ServiceProviders;
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Foundations.Rules;

internal class RuleEvaluationService(
    IServiceProviderBroker serviceProviderBroker,
    ISTXSTRUCTRulesProcessingService structuralRulesProcessingService)
    : IRuleEvaluationService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        if (context.Declarations.Any(
            predicate: declaration => declaration is InterfaceDeclarationSyntax))
        {
            return structuralRulesProcessingService.Evaluate(context: context);
        }

        IEnumerable<IRuleProcessingService> ruleHandlingServices = serviceProviderBroker.GetRuleHandlingServices(
            standardElementType: context.StandardElementType
        );

        return ruleHandlingServices.SelectMany(
            selector: (IRuleProcessingService ruleHandlingService) => ruleHandlingService.Evaluate(context: context)
        );
    }
}