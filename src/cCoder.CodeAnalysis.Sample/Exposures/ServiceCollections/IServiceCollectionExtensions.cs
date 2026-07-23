// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Services.Processings.ServiceCollections;
using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Exposures.ServiceCollections;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddSchool(
        this IServiceCollection services,
        School school) =>
        services;

    public static IServiceCollection AddCodeAnalysisSample(this IServiceCollection services, string connectionString) =>
        new ServiceCollectionProcessingService().AddCodeAnalysisSample(
            services: services,
            connectionString: connectionString
        );
}