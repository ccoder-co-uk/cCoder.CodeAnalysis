// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;

namespace cCoder.CodeAnalysis.Brokers.ServiceProviders;

internal interface IServiceProviderBroker
{
    IEnumerable<IRuleProcessingService> GetRuleHandlingServices(StandardElementType standardElementType);
}