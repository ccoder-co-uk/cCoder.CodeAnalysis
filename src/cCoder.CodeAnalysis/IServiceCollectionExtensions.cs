// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Brokers.Files;
using cCoder.CodeAnalysis.Brokers.ServiceProviders;
using cCoder.CodeAnalysis.Exposures;
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Foundations.Architectures;
using cCoder.CodeAnalysis.Services.Foundations.Rules;
using cCoder.CodeAnalysis.Services.Orchestrations.Architectures;
using cCoder.CodeAnalysis.Services.Processings.Architectures;
using cCoder.CodeAnalysis.Services.Processings.Contexts;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using Microsoft.Extensions.DependencyInjection;

namespace cCoder.CodeAnalysis;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddCodeAnalysis(this IServiceCollection services)
    {
        AddCodeAnalysisBrokers(services: services);
        AddCodeAnalysisFoundations(services: services);
        AddCodeAnalysisProcessings(services: services);
        AddCodeAnalysisOrchestrations(services: services);
        AddCodeAnalysisExposures(services: services);
        return services;
    }

    private static void AddCodeAnalysisBrokers(IServiceCollection services)
    {
        services.AddScoped<IFileBroker, FileBroker>();
        services.AddSingleton<IServiceProviderBroker, ServiceProviderBroker>();
    }

    private static void AddCodeAnalysisFoundations(IServiceCollection services)
    {
        services.AddScoped<IArchitectureService, ArchitectureService>();
        services.AddSingleton<IRuleEvaluationService, RuleEvaluationService>();
    }

    private static void AddCodeAnalysisProcessings(IServiceCollection services)
    {
        services.AddScoped<IArchitectureProcessingService, ArchitectureProcessingService>();
        services.AddSingleton<IEvaluationContextsProcessingService, EvaluationContextsProcessingService>();
        services.AddSingleton<ISTXRulesProcessingService, STXRulesProcessingService>();
        services.AddSingleton<ISTXAPPRulesProcessingService, STXAPPRulesProcessingService>();
        services.AddSingleton<ISTXAPIRulesProcessingService, STXAPIRulesProcessingService>();
        services.AddSingleton<ISTXBRulesProcessingService, STXBRulesProcessingService>();
        services.AddSingleton<ISTXDRulesProcessingService, STXDRulesProcessingService>();
        services.AddSingleton<ISTXERulesProcessingService, STXERulesProcessingService>();
        services.AddSingleton<ISTXEXRulesProcessingService, STXEXRulesProcessingService>();
        services.AddSingleton<ISTXFORMATRulesProcessingService, STXFORMATRulesProcessingService>();
        services.AddSingleton<ISTXSTRUCTRulesProcessingService, STXSTRUCTRulesProcessingService>();
        services.AddSingleton<ISTXMRulesProcessingService, STXMRulesProcessingService>();
        services.AddSingleton<ISTXTESTRulesProcessingService, STXTESTRulesProcessingService>();
        services.AddSingleton<ISTXFRulesProcessingService, STXFRulesProcessingService>();
        services.AddSingleton<ISTXPRulesProcessingService, STXPRulesProcessingService>();
        services.AddSingleton<ISTXORulesProcessingService, STXORulesProcessingService>();
        services.AddSingleton<ISTXCRulesProcessingService, STXCRulesProcessingService>();
        services.AddSingleton<ISTXMGRulesProcessingService, STXMGRulesProcessingService>();
        services.AddSingleton<ISTXARulesProcessingService, STXARulesProcessingService>();
        AddRule<ISTXRulesProcessingService>(services: services, "STX");
        AddRule<ISTXAPPRulesProcessingService>(services: services, "STXAPP");
        AddRule<ISTXAPIRulesProcessingService>(services: services, "STXAPI");
        AddRule<ISTXARulesProcessingService>(services: services, "STXA");
        AddRule<ISTXBRulesProcessingService>(services: services, "STXB");
        AddRule<ISTXCRulesProcessingService>(services: services, "STXC");
        AddRule<ISTXDRulesProcessingService>(services: services, "STXD");
        AddRule<ISTXERulesProcessingService>(services: services, "STXE");
        AddRule<ISTXEXRulesProcessingService>(services: services, "STXEX");
        AddRule<ISTXFRulesProcessingService>(services: services, "STXF");
        AddRule<ISTXFORMATRulesProcessingService>(services: services, "STXFORMAT");
        AddRule<ISTXMGRulesProcessingService>(services: services, "STXMG");
        AddRule<ISTXMRulesProcessingService>(services: services, "STXM");
        AddRule<ISTXORulesProcessingService>(services: services, "STXO");
        AddRule<ISTXPRulesProcessingService>(services: services, "STXP");
        AddRule<ISTXSTRUCTRulesProcessingService>(services: services, "STXSTRUCT");
        AddRule<ISTXTESTRulesProcessingService>(services: services, "STXTEST");
        AddRuleHandlingServices(services: services);
        services.AddSingleton<IRuleEvaluationsProcessingService, RuleEvaluationsProcessingService>();
    }

    private static void AddCodeAnalysisOrchestrations(IServiceCollection services)
    {
        services.AddScoped<IArchitectureOrchestrationService, ArchitectureOrchestrationService>();
    }

    private static void AddCodeAnalysisExposures(IServiceCollection services)
    {
        services.AddScoped<IArchitectureBuilder, ArchitectureBuilder>();
    }

    private static void AddRule<TRule>(IServiceCollection services, params string[] prefixes)
        where TRule : class, IRuleProcessingService
    {
        services.AddSingleton<IRuleProcessingService>(
            implementationFactory: (IServiceProvider serviceProvider) => serviceProvider.GetRequiredService<TRule>()
        );

        foreach (string prefix in prefixes)
        {
            services.AddKeyedSingleton<IRuleProcessingService>(
                serviceKey: prefix,
                implementationFactory: (IServiceProvider serviceProvider, object? _) =>
                    serviceProvider.GetRequiredService<TRule>()
            );
        }
    }

    private static void AddRuleHandlingServices(IServiceCollection services)
    {
        AddRuleHandlingServices<ISTXAPPRulesProcessingService>(
            services: services,
            standardElementType: StandardElementType.App
        );

        AddRuleHandlingServices<ISTXFORMATRulesProcessingService>(
            services: services,
            standardElementType: StandardElementType.Activity
        );

        AddRuleHandlingServices<ISTXDRulesProcessingService>(
            services: services,
            standardElementType: StandardElementType.Dependency
        );

        AddRuleHandlingServices<ISTXFORMATRulesProcessingService, ISTXMRulesProcessingService>(
            services: services,
            standardElementType: StandardElementType.Model
        );

        AddRuleHandlingServices<
            ISTXFORMATRulesProcessingService,
            ISTXSTRUCTRulesProcessingService,
            ISTXTESTRulesProcessingService
        >(services: services, standardElementType: StandardElementType.Test);

        AddRuleHandlingServices<
            ISTXFORMATRulesProcessingService,
            ISTXSTRUCTRulesProcessingService,
            ISTXRulesProcessingService,
            ISTXBRulesProcessingService
        >(services: services, standardElementType: StandardElementType.Broker);

        AddRuleHandlingServices<
            ISTXFORMATRulesProcessingService,
            ISTXSTRUCTRulesProcessingService,
            ISTXRulesProcessingService,
            ISTXERulesProcessingService,
            ISTXAPIRulesProcessingService
        >(services: services, standardElementType: StandardElementType.Exposure);

        AddServiceRuleHandlingServices<ISTXARulesProcessingService>(
            services: services,
            standardElementType: StandardElementType.AggregationService
        );

        AddServiceRuleHandlingServices<ISTXCRulesProcessingService>(
            services: services,
            standardElementType: StandardElementType.CoordinationService
        );

        AddServiceRuleHandlingServices<ISTXFRulesProcessingService>(
            services: services,
            standardElementType: StandardElementType.FoundationService
        );

        AddServiceRuleHandlingServices<ISTXMGRulesProcessingService>(
            services: services,
            standardElementType: StandardElementType.ManagementService
        );

        AddServiceRuleHandlingServices<ISTXORulesProcessingService>(
            services: services,
            standardElementType: StandardElementType.OrchestrationService
        );

        AddServiceRuleHandlingServices<ISTXPRulesProcessingService>(
            services: services,
            standardElementType: StandardElementType.ProcessingService
        );

        AddRuleHandlingServices<ISTXRulesProcessingService, ISTXDRulesProcessingService>(
            services: services,
            standardElementType: StandardElementType.Unknown
        );
    }

    private static void AddServiceRuleHandlingServices<TElementRule>(
        IServiceCollection services,
        StandardElementType standardElementType
    )
        where TElementRule : class, IRuleProcessingService =>

        AddRuleHandlingServices<
            ISTXFORMATRulesProcessingService,
            ISTXSTRUCTRulesProcessingService,
            ISTXRulesProcessingService,
            ISTXEXRulesProcessingService,
            TElementRule
        >(services: services, standardElementType: standardElementType);

    private static void AddRuleHandlingServices<T1>(
        IServiceCollection services,
        StandardElementType standardElementType
    )
        where T1 : class, IRuleProcessingService =>

        AddRuleHandlingServices(
            services: services,
            standardElementType: standardElementType,
            ruleServicesFactory: (IServiceProvider serviceProvider) => [serviceProvider.GetRequiredService<T1>()]
        );

    private static void AddRuleHandlingServices<T1, T2>(
        IServiceCollection services,
        StandardElementType standardElementType
    )
        where T1 : class, IRuleProcessingService
        where T2 : class, IRuleProcessingService =>

        AddRuleHandlingServices(
            services: services,
            standardElementType: standardElementType,
            ruleServicesFactory: (IServiceProvider serviceProvider) =>
                [serviceProvider.GetRequiredService<T1>(), serviceProvider.GetRequiredService<T2>()]
        );

    private static void AddRuleHandlingServices<T1, T2, T3>(
        IServiceCollection services,
        StandardElementType standardElementType
    )
        where T1 : class, IRuleProcessingService
        where T2 : class, IRuleProcessingService
        where T3 : class, IRuleProcessingService =>

        AddRuleHandlingServices(
            services: services,
            standardElementType: standardElementType,
            ruleServicesFactory: (IServiceProvider serviceProvider) =>
                [
                    serviceProvider.GetRequiredService<T1>(),
                    serviceProvider.GetRequiredService<T2>(),
                    serviceProvider.GetRequiredService<T3>(),
                ]
        );

    private static void AddRuleHandlingServices<T1, T2, T3, T4>(
        IServiceCollection services,
        StandardElementType standardElementType
    )
        where T1 : class, IRuleProcessingService
        where T2 : class, IRuleProcessingService
        where T3 : class, IRuleProcessingService
        where T4 : class, IRuleProcessingService =>

        AddRuleHandlingServices(
            services: services,
            standardElementType: standardElementType,
            ruleServicesFactory: (IServiceProvider serviceProvider) =>
                [
                    serviceProvider.GetRequiredService<T1>(),
                    serviceProvider.GetRequiredService<T2>(),
                    serviceProvider.GetRequiredService<T3>(),
                    serviceProvider.GetRequiredService<T4>(),
                ]
        );

    private static void AddRuleHandlingServices<T1, T2, T3, T4, T5>(
        IServiceCollection services,
        StandardElementType standardElementType
    )
        where T1 : class, IRuleProcessingService
        where T2 : class, IRuleProcessingService
        where T3 : class, IRuleProcessingService
        where T4 : class, IRuleProcessingService
        where T5 : class, IRuleProcessingService =>

        AddRuleHandlingServices(
            services: services,
            standardElementType: standardElementType,
            ruleServicesFactory: (IServiceProvider serviceProvider) =>
                [
                    serviceProvider.GetRequiredService<T1>(),
                    serviceProvider.GetRequiredService<T2>(),
                    serviceProvider.GetRequiredService<T3>(),
                    serviceProvider.GetRequiredService<T4>(),
                    serviceProvider.GetRequiredService<T5>(),
                ]
        );

    private static void AddRuleHandlingServices(
        IServiceCollection services,
        StandardElementType standardElementType,
        Func<IServiceProvider, IEnumerable<IRuleProcessingService>> ruleServicesFactory
    ) =>

        services.AddKeyedSingleton<IEnumerable<IRuleProcessingService>>(
            serviceKey: standardElementType.ToString(),
            implementationFactory: (IServiceProvider serviceProvider, object? _) =>
                ruleServicesFactory(arg: serviceProvider)
            .Append(element: serviceProvider.GetRequiredService<ISTXDRulesProcessingService>())
                    .Distinct()
        );
}