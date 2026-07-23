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
        services
            .AddControllersWithViews(options =>
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true
            )
            .AddApplicationPart(typeof(StudentsController).Assembly);

        return new ServiceCollectionProcessingService().AddCodeAnalysisSample(
            services: services,
            connectionString: connectionString
        );
    }
}
