// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Brokers.ServiceProviders;
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Foundations.Rules;

internal class RuleEvaluationService(IServiceProviderBroker serviceProviderBroker) : IRuleEvaluationService
{
    private static readonly IArchitectureModelQueriesProcessingService architectureModelQueries =
        new ArchitectureModelQueriesProcessingService();

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        bool isInterface = architectureModelQueries.GetDeclarations(context: context).Any(
            predicate: declaration => declaration is InterfaceDeclarationSyntax);

        IEnumerable<IRuleProcessingService> ruleHandlingServices = isInterface
            ? serviceProviderBroker.GetStructuralRuleHandlingServices()
            : serviceProviderBroker.GetRuleHandlingServices(
                standardElementType: architectureModelQueries.GetStandardElementType(context: context));

        return ruleHandlingServices.SelectMany(
            selector: (IRuleProcessingService ruleHandlingService) => ruleHandlingService.Evaluate(context: context)
        );
    }
}
