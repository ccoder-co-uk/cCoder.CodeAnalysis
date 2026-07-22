// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Services.Processings.ServiceCollections;

internal interface IServiceCollectionProcessingService
{
    IServiceCollection AddCodeAnalysisSample(IServiceCollection services, string connectionString);
}