// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Services.Processings.ServiceCollections;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Processings.ServiceCollections;

public sealed partial class ServiceCollectionProcessingServiceTests
{
    [Fact]
    public void AddCodeAnalysisSampleRegistersServices()
    {
        // Given
        // When
        // Then
        ServiceCollection services = new ServiceCollection();
        ServiceCollectionProcessingService service = CreateServiceCollectionProcessingService();

        IServiceCollection registeredServices = service.AddCodeAnalysisSample(
services: services,
connectionString: "Server=(localdb)\\MSSQLLocalDB;Database=RegistrationTests;Trusted_Connection=True"
        );

        ((IEnumerable<ServiceDescriptor>)registeredServices).Should()
            .BeSameAs(expected: services, because: "");

        ((IEnumerable<ServiceDescriptor>)registeredServices).Should()
            .NotBeEmpty(because: "");
    }
}