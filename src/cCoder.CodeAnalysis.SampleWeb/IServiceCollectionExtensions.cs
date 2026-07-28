// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample;
using cCoder.CodeAnalysis.SampleWeb.Models;

namespace cCoder.CodeAnalysis.SampleWeb;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddSampleWeb(
        this IServiceCollection services,
        IConfiguration applicationConfiguration,
        Action<SampleWebConfiguration>? configure = null)
    {
        SampleWebConfiguration configuration = new();
        applicationConfiguration.Bind(configuration);
        configure?.Invoke(configuration);
        services.AddCodeAnalysisSampleWeb(
            configuration.CodeAnalysisSample);

        return services;
    }
}