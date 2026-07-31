// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Reflection;
using cCoder.CodeAnalysis.Exposures;
using cCoder.CodeAnalysis.Models;
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
        Type[] unexpectedTypes = sampleAssembly.GetExportedTypes()
            .Where(
                (Type type) =>
                !type.Namespace!.Contains(".Models", StringComparison.Ordinal)
                && !type.Namespace.Contains(".Exposures", StringComparison.Ordinal)
                && !type.Namespace.Contains(".Controllers", StringComparison.Ordinal)
                && !type.Namespace.Contains(".RuleViolations", StringComparison.Ordinal)
                && type.Name != "IServiceCollectionExtensions"
            )
            .ToArray();
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
        Type[] unexpectedTypes = codeAnalysisAssembly.GetExportedTypes()
            .Where(
                (Type type) =>
                !type.Namespace!.Contains(".Models", StringComparison.Ordinal)
                && !type.Namespace.Contains(".Exposures", StringComparison.Ordinal)
                && !type.Namespace.Contains(".Analyzers", StringComparison.Ordinal)
                && type.Name != "IServiceCollectionExtensions"
            )
            .ToArray();
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

        provider.GetServices<IRuleProcessingService>().Should().HaveCount(18, "");
    }

    [Theory]
    [InlineData("STX", typeof(ISTXRulesProcessingService))]
    [InlineData("STXAPP", typeof(ISTXAPPRulesProcessingService))]
    [InlineData("STXAPI", typeof(ISTXAPIRulesProcessingService))]
    [InlineData("RFC", typeof(IRFCRulesProcessingService))]
    [InlineData("STXA", typeof(ISTXARulesProcessingService))]
    [InlineData("STXB", typeof(ISTXBRulesProcessingService))]
    [InlineData("STXC", typeof(ISTXCRulesProcessingService))]
    [InlineData("STXD", typeof(ISTXDRulesProcessingService))]
    [InlineData("STXE", typeof(ISTXERulesProcessingService))]
    [InlineData("STXEX", typeof(ISTXEXRulesProcessingService))]
    [InlineData("STXF", typeof(ISTXFRulesProcessingService))]
    [InlineData("STXFORMAT", typeof(ISTXFORMATRulesProcessingService))]
    [InlineData("STXM", typeof(ISTXMRulesProcessingService))]
    [InlineData("STXMG", typeof(ISTXMGRulesProcessingService))]
    [InlineData("STXO", typeof(ISTXORulesProcessingService))]
    [InlineData("STXP", typeof(ISTXPRulesProcessingService))]
    [InlineData("STXSTRUCT", typeof(ISTXSTRUCTRulesProcessingService))]
    [InlineData("STXTEST", typeof(ISTXTESTRulesProcessingService))]
    public void AddCodeAnalysisShouldRegisterRulesByPrefix(string prefix, Type expectedServiceType)
    {
        ServiceCollection services = new ServiceCollection();
        services.AddCodeAnalysis();
        using ServiceProvider provider = services.BuildServiceProvider();

        IRuleProcessingService rule = provider.GetRequiredKeyedService<IRuleProcessingService>(prefix);

        expectedServiceType.IsInstanceOfType(rule).Should().BeTrue("");
    }

    [Fact]
    public void AddCodeAnalysisShouldEvaluateRfcRulesForExposures()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddCodeAnalysis();
        using ServiceProvider provider = services.BuildServiceProvider();

        IEnumerable<IRuleProcessingService> exposureRules =
            provider.GetRequiredKeyedService<
                IEnumerable<IRuleProcessingService>>(
                StandardElementType.Exposure.ToString());

        exposureRules.Should()
            .ContainSingle(rule => rule is IRFCRulesProcessingService);
    }
}
