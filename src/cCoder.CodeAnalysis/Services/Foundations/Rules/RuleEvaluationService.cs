// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Brokers.ServiceProviders;
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;

namespace cCoder.CodeAnalysis.Services.Foundations.Rules;

internal class RuleEvaluationService(IServiceProviderBroker serviceProviderBroker) : IRuleEvaluationService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        IEnumerable<IRuleProcessingService> ruleHandlingServices = serviceProviderBroker.GetRuleHandlingServices(
            standardElementType: context.StandardElementType
        );

        return ruleHandlingServices.SelectMany(
            selector: (IRuleProcessingService ruleHandlingService) => ruleHandlingService.Evaluate(context: context)
        );
    }
}