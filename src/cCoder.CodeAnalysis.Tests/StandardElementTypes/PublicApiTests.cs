// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Reflection;
using cCoder.CodeAnalysis.Exposures;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace cCoder.CodeAnalysis.Tests.StandardElementTypes;

public sealed class PublicApiTests
{
    [Fact]
    public void SampleShouldOnlyExposeModelsExposuresAndRegistration()
    {
        Assembly sampleAssembly = typeof(Student).Assembly;
        Type[] unexpectedTypes = (
            from type in sampleAssembly.GetExportedTypes()
            where
                !type.Namespace!.Contains(".Models", StringComparison.Ordinal)
                && !type.Namespace.Contains(".Exposures", StringComparison.Ordinal)
                && !type.Namespace.Contains(".Controllers", StringComparison.Ordinal)
                && !type.Namespace.Contains(".RuleViolations", StringComparison.Ordinal)
                && type.Name != "IServiceCollectionExtensions"
            select type
        ).ToArray();
        ((IEnumerable<Type>)unexpectedTypes).Should().BeEmpty("");
        ((IEnumerable<Type>)sampleAssembly.GetExportedTypes())
            .Should()
            .OnlyContain(
                (Type type) =>
                    (
                        !type.Namespace!.Contains(".Exposures", StringComparison.Ordinal)
                        && !type.Namespace.Contains(".Controllers", StringComparison.Ordinal)
                    )
                    || type.IsInterface
                    || type.Namespace.Contains(".Controllers", StringComparison.Ordinal)
                    || type.Name == "IServiceCollectionExtensions",
                ""
            );
    }

    [Fact]
    public void CodeAnalysisShouldOnlyExposeModelsExposuresAndRegistration()
    {
        Assembly codeAnalysisAssembly = typeof(ArchitectureBuilder).Assembly;
        Type[] unexpectedTypes = (
            from type in codeAnalysisAssembly.GetExportedTypes()
            where
                !type.Namespace!.Contains(".Models", StringComparison.Ordinal)
                && !type.Namespace.Contains(".Exposures", StringComparison.Ordinal)
                && !type.Namespace.Contains(".Analyzers", StringComparison.Ordinal)
                && type.Name != "IServiceCollectionExtensions"
            select type
        ).ToArray();
        ((IEnumerable<Type>)unexpectedTypes).Should().BeEmpty("");
        ((IEnumerable<Type>)codeAnalysisAssembly.GetExportedTypes())
            .Should()
            .OnlyContain(
                (Type type) => !type.Namespace!.Contains(".Exposures", StringComparison.Ordinal) || type.IsInterface,
                ""
            );
    }

    [Fact]
    public void AddCodeAnalysisShouldRegisterArchitectureBuilder()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddCodeAnalysis();
        using ServiceProvider provider = services.BuildServiceProvider();
        ((object)provider.GetRequiredService<IArchitectureBuilder>()).Should().NotBeNull("");
    }

    [Fact]
    public void AddCodeAnalysisShouldRegisterAllRuleProcessingServices()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddCodeAnalysis();
        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetServices<IRuleProcessingService>().Should().HaveCount(11, "");
    }

    [Theory]
    [InlineData("STXA", typeof(IAggregationServiceCodeAnalysisRulesProcessingService))]
    [InlineData("STXB", typeof(IBrokerCodeAnalysisRulesProcessingService))]
    [InlineData("STXC", typeof(ICoordinationServiceCodeAnalysisRulesProcessingService))]
    [InlineData("STXD", typeof(IDependencyCodeAnalysisRulesProcessingService))]
    [InlineData("STXE", typeof(IExposureCodeAnalysisRulesProcessingService))]
    [InlineData("STXF", typeof(IFoundationServiceCodeAnalysisRulesProcessingService))]
    [InlineData("STXM", typeof(IModelCodeAnalysisRulesProcessingService))]
    [InlineData("STXMG", typeof(IManagementServiceCodeAnalysisRulesProcessingService))]
    [InlineData("STXO", typeof(IOrchestrationServiceCodeAnalysisRulesProcessingService))]
    [InlineData("STXP", typeof(IProcessingServiceCodeAnalysisRulesProcessingService))]
    [InlineData("STXTEST", typeof(ITestCodeAnalysisRulesProcessingService))]
    public void AddCodeAnalysisShouldRegisterRulesByPrefix(string prefix, Type expectedServiceType)
    {
        ServiceCollection services = new ServiceCollection();
        services.AddCodeAnalysis();
        using ServiceProvider provider = services.BuildServiceProvider();

        IRuleProcessingService rule = provider.GetRequiredKeyedService<IRuleProcessingService>(prefix);

        expectedServiceType.IsInstanceOfType(rule).Should().BeTrue("");
    }
}
