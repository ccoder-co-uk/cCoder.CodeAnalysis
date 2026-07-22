// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Reflection;
using cCoder.CodeAnalysis.Exposures;
using cCoder.CodeAnalysis.Sample.Models.Schools;
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
}