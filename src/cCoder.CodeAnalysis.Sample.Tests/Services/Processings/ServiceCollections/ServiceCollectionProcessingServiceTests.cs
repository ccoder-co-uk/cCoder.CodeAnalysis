// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Services.Processings.ServiceCollections;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Processings.ServiceCollections;

public sealed partial class ServiceCollectionProcessingServiceTests
{
    private static ServiceCollectionProcessingService CreateServiceCollectionProcessingService()
    {
        return new ServiceCollectionProcessingService();
    }
}