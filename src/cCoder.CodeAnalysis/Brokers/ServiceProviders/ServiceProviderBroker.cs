// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using Microsoft.Extensions.DependencyInjection;

namespace cCoder.CodeAnalysis.Brokers.ServiceProviders;

internal sealed class ServiceProviderBroker(IServiceProvider serviceProvider) : IServiceProviderBroker
{
    public IEnumerable<IRuleProcessingService> GetRuleHandlingServices(StandardElementType standardElementType) =>

        serviceProvider.GetRequiredKeyedService<IEnumerable<IRuleProcessingService>>(
            serviceKey: standardElementType.ToString()
        );
}