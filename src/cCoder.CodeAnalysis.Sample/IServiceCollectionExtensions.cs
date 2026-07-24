// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Services.Processings.ServiceCollections;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Controllers;

namespace cCoder.CodeAnalysis.Sample;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddSchool(
        this IServiceCollection services,
        School school) =>
        services;

    public static IServiceCollection AddCodeAnalysisSample(this IServiceCollection services, string connectionString)
    {
        AddCodeAnalysisSampleExposures(services: services);

        return AddCodeAnalysisSampleProcessings(
            services: services,
            connectionString: connectionString
        );
    }

    private static void AddCodeAnalysisSampleExposures(IServiceCollection services)
    {
        IMvcBuilder controllers = services.AddControllersWithViews(
            configure: options =>
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true
        );

        controllers.AddApplicationPart(assembly: typeof(StudentsController).Assembly);
    }

    private static IServiceCollection AddCodeAnalysisSampleProcessings(
        IServiceCollection services,
        string connectionString
    ) =>

        new ServiceCollectionProcessingService().AddCodeAnalysisSample(
            services: services,
            connectionString: connectionString
        );
}
