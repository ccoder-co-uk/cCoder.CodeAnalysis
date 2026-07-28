// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Services.Processings.ServiceCollections;
using cCoder.CodeAnalysis.Sample.Controllers;
using cCoder.CodeAnalysis.Sample.Models;

namespace cCoder.CodeAnalysis.Sample;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddCodeAnalysisSampleWeb(
        this IServiceCollection services,
        CodeAnalysisSampleConfiguration configuration)
    {
        services.AddExposures();
        services.AddProcessings(configuration);

        return services;
    }

    private static void AddExposures(
        this IServiceCollection services)
    {
        IMvcBuilder controllers = services.AddControllersWithViews(
            configure: options =>
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true
        );

        controllers.AddApplicationPart(assembly: typeof(StudentsController).Assembly);
    }

    private static void AddProcessings(
        this IServiceCollection services,
        CodeAnalysisSampleConfiguration configuration) =>
        new ServiceCollectionProcessingService()
            .AddCodeAnalysisSample(
            services: services,
            connectionString: configuration.ConnectionString);
}