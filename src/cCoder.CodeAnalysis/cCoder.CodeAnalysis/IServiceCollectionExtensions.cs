// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Brokers.Files;
using cCoder.CodeAnalysis.Exposures;
using cCoder.CodeAnalysis.Services.Coordinations.Rules;
using cCoder.CodeAnalysis.Services.Foundations.Architectures;
using cCoder.CodeAnalysis.Services.Foundations.Projects;
using cCoder.CodeAnalysis.Services.Orchestrations.Architectures;
using cCoder.CodeAnalysis.Services.Orchestrations.Rules;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using Microsoft.Extensions.DependencyInjection;

namespace cCoder.CodeAnalysis;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddCodeAnalysis(this IServiceCollection services)
    {
        services.AddScoped<IFileBroker, FileBroker>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IArchitectureService, ArchitectureService>();
        services.AddScoped<ArchitectureJsonSerializer>();
        services.AddScoped<IArchitectureOrchestrationService, ArchitectureOrchestrationService>();
        services.AddScoped<IBrokerCodeAnalysisRulesProcessingService, BrokerCodeAnalysisRulesProcessingService>();
        services.AddScoped<
            IDependencyCodeAnalysisRulesProcessingService,
            DependencyCodeAnalysisRulesProcessingService
        >();
        services.AddScoped<IExposureCodeAnalysisRulesProcessingService, ExposureCodeAnalysisRulesProcessingService>();
        services.AddScoped<IModelCodeAnalysisRulesProcessingService, ModelCodeAnalysisRulesProcessingService>();
        services.AddScoped<ITestCodeAnalysisRulesProcessingService, TestCodeAnalysisRulesProcessingService>();
        services.AddScoped<
            IFoundationServiceCodeAnalysisRulesProcessingService,
            FoundationServiceCodeAnalysisRulesProcessingService
        >();
        services.AddScoped<
            IProcessingServiceCodeAnalysisRulesProcessingService,
            ProcessingServiceCodeAnalysisRulesProcessingService
        >();
        services.AddScoped<
            IOrchestrationServiceCodeAnalysisRulesProcessingService,
            OrchestrationServiceCodeAnalysisRulesProcessingService
        >();
        services.AddScoped<
            ICoordinationServiceCodeAnalysisRulesProcessingService,
            CoordinationServiceCodeAnalysisRulesProcessingService
        >();
        services.AddScoped<
            IManagementServiceCodeAnalysisRulesProcessingService,
            ManagementServiceCodeAnalysisRulesProcessingService
        >();
        services.AddScoped<
            IAggregationServiceCodeAnalysisRulesProcessingService,
            AggregationServiceCodeAnalysisRulesProcessingService
        >();
        services.AddScoped<
            ICulDeSacServicesAndBrokerRuleEvaluationOrchestrationService,
            CulDeSacServicesAndBrokerRuleEvaluationOrchestrationService
        >();
        services.AddScoped<
            IHigherLevelServicesRuleEvaluationOrchestrationService,
            HigherLevelServicesRuleEvaluationOrchestrationService
        >();
        services.AddScoped<
            IExposuresAndModelsRuleEvaluationOrchestrationService,
            ExposuresAndModelsRuleEvaluationOrchestrationService
        >();
        services.AddScoped<IRuleEvaluationCoordinationService, RuleEvaluationCoordinationService>();
        services.AddScoped<IArchitectureBuilder, ArchitectureBuilder>();
        return services;
    }
}