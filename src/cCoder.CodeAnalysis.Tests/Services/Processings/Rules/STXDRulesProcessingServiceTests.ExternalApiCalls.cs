// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Foundations.Architectures;
using cCoder.CodeAnalysis.Services.Processings.Architectures;
using cCoder.CodeAnalysis.Services.Processings.Contexts;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Moq;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed partial class STXDRulesProcessingServiceTests
{
    [Fact]
    public void ExternalApiCall_WhenMadeFromFoundationService_IsReported() =>
        AssertExternalApiCallIsReported(
            namespaceName: "Example.Services.Foundations",
            typeName: "ExampleService");

    [Fact]
    public void ExternalApiCall_WhenMadeFromProcessingService_IsReported() =>
        AssertExternalApiCallIsReported(
            namespaceName: "Example.Services.Processings",
            typeName: "ExampleProcessingService");

    [Fact]
    public void ExternalApiCall_WhenMadeFromOrchestrationService_IsReported() =>
        AssertExternalApiCallIsReported(
            namespaceName: "Example.Services.Orchestrations",
            typeName: "ExampleOrchestrationService");

    [Fact]
    public void ExternalApiCall_WhenMadeFromCoordinationService_IsReported() =>
        AssertExternalApiCallIsReported(
            namespaceName: "Example.Services.Coordinations",
            typeName: "ExampleCoordinationService");

    [Fact]
    public void ExternalApiCall_WhenMadeFromAggregationService_IsReported() =>
        AssertExternalApiCallIsReported(
            namespaceName: "Example.Services.Aggregations",
            typeName: "ExampleAggregationService");

    [Fact]
    public void ExternalApiCall_WhenMadeFromExposure_IsReported() =>
        AssertExternalApiCallIsReported(
            namespaceName: "Example.Exposures",
            typeName: "ExampleManager");

    [Fact]
    public void ExternalApiCall_WhenMadeFromHttpExposure_IsReported()
    {
        EvaluationContext context = CreateExternalApiContext(
            source:
                "namespace Example.Controllers; "
                + "public sealed class ExampleController "
                + "{ public void Patch(ThirdParty.Delta<ExampleController> delta) "
                + "=> delta.Patch(this); }",
            externalSource:
                "namespace ThirdParty; public sealed class Delta<T> "
                + "{ public void Patch(T target) { } }",
            typeName: "Example.Controllers.ExampleController");

        new STXDRulesProcessingService().Evaluate(context)
            .Should().ContainSingle(item => item.Code == "STXD005");
    }

    [Fact]
    public void ExternalApiCall_WhenMadeFromBroker_IsNotReported() =>
        AssertExternalApiCallIsNotReported(
            namespaceName: "Example.Brokers",
            typeName: "ExampleBroker");

    [Fact]
    public void ExternalApiCall_WhenMadeFromDependency_IsNotReported() =>
        AssertExternalApiCallIsNotReported(
            namespaceName: "Example.Dependencies",
            typeName: "ExampleDependency");

    [Fact]
    public void ExternalApiCall_WhenMadeFromCompositionRoot_IsNotReported() =>
        AssertExternalApiCallIsNotReported(
            namespaceName: "Example",
            typeName: "IServiceCollectionExtensions");

    [Fact]
    public void ExternalApiCall_WhenMadeFromServiceCollectionComposition_IsNotReported()
    {
        EvaluationContext context = CreateExternalApiContext(
            source:
                "namespace Example.Services.Processings; "
                + "public sealed class ServiceCollectionProcessingService "
                + "{ public string Execute() => ThirdParty.ExternalApi.Serialize(this); }",
            externalSource:
                "namespace ThirdParty; public static class ExternalApi "
                + "{ public static string Serialize(object value) => string.Empty; }",
            typeName:
                "Example.Services.Processings.ServiceCollectionProcessingService");

        Method method = context.ArchitectureElement.AnalysisMethods.Single();
        method.ReturnType = "IServiceCollection";
        method.Inputs.Add(new Input
        {
            Name = "services",
            Type = "IServiceCollection",
        });

        new STXDRulesProcessingService().Evaluate(context)
            .Should().NotContain(item => item.Code == "STXD005");
    }

    [Fact]
    public void ExternalApiCall_WhenMadeFromMvcBuilderComposition_IsNotReported()
    {
        EvaluationContext context = CreateExternalApiContext(
            source:
                "namespace Example.Exposures; "
                + "public static class IMvcBuilderExtensions "
                + "{ public static IMvcBuilder AddFeature(IMvcBuilder mvcBuilder) "
                + "=> ThirdParty.ExternalApi.Configure(mvcBuilder); }",
            externalSource:
                "public interface IMvcBuilder { } "
                + "namespace ThirdParty { public static class ExternalApi "
                + "{ public static IMvcBuilder Configure(IMvcBuilder mvcBuilder) => mvcBuilder; } }",
            typeName: "Example.Exposures.IMvcBuilderExtensions");

        Method method = context.ArchitectureElement.AnalysisMethods.Single();
        method.ReturnType = "IMvcBuilder";
        method.Inputs.Add(new Input
        {
            Name = "mvcBuilder",
            Type = "IMvcBuilder",
        });

        new STXDRulesProcessingService().Evaluate(context)
            .Should().NotContain(item => item.Code == "STXD005");
    }

    [Fact]
    public void ExternalApiCall_WhenMadeFromApplicationBuilderComposition_IsNotReported()
    {
        EvaluationContext context = CreateExternalApiContext(
            source:
                "namespace Example.Exposures; "
                + "public static class IApplicationBuilderExtensions "
                + "{ public static ThirdParty.IApplicationBuilder UseFeature("
                + "this ThirdParty.IApplicationBuilder builder) "
                + "=> builder.UseMiddleware(); }",
            externalSource:
                "namespace ThirdParty; public interface IApplicationBuilder "
                + "{ IApplicationBuilder UseMiddleware(); }",
            typeName: "Example.Exposures.IApplicationBuilderExtensions");

        new STXDRulesProcessingService().Evaluate(context)
            .Should().NotContain(item => item.Code == "STXD005");
    }

    [Fact]
    public void ExternalRequestDelegate_WhenInvokedByMiddleware_IsNotReported()
    {
        EvaluationContext context = CreateExternalApiContext(
            source:
                "namespace Example.Exposures; "
                + "public sealed class ExampleMiddleware "
                + "{ private readonly ThirdParty.RequestDelegate next; "
                + "public ExampleMiddleware(ThirdParty.RequestDelegate next) "
                + "{ this.next = next; } "
                + "public System.Threading.Tasks.Task InvokeAsync(object context) "
                + "=> next(context); }",
            externalSource:
                "namespace ThirdParty; public delegate "
                + "System.Threading.Tasks.Task RequestDelegate(object context);",
            typeName: "Example.Exposures.ExampleMiddleware");

        new STXDRulesProcessingService().Evaluate(context)
            .Should().NotContain(item => item.Code == "STXD005");
    }

    [Fact]
    public void ExternalApiCall_WhenMadeFromMiddleware_IsReported() =>
        AssertExternalApiCallIsReported(
            namespaceName: "Example.Exposures",
            typeName: "ExampleMiddleware");

    [Fact]
    public void ExternalApiCall_WhenMadeFromODataModelBuilder_IsNotReported() =>
        AssertExternalApiCallIsNotReported(
            namespaceName: "Example.Exposures",
            typeName: "ODataModelBuilder");

    [Fact]
    public void ExternalApiCall_WhenMadeFromODataBuilderComposition_IsNotReported()
    {
        EvaluationContext context = CreateExternalApiContext(
            source:
                "namespace Example.Exposures; "
                + "public static class ODataConventionModelBuilderExtensions "
                + "{ public static void Configure(object builder) "
                + "=> ThirdParty.ExternalApi.Configure(builder); }",
            externalSource:
                "namespace ThirdParty; public static class ExternalApi "
                + "{ public static void Configure(object builder) { } }",
            typeName:
                "Example.Exposures.ODataConventionModelBuilderExtensions");

        Method method = context.ArchitectureElement.AnalysisMethods.Single();
        method.ReturnType = "void";
        method.Inputs.Single().Type = "ODataConventionModelBuilder";

        new STXDRulesProcessingService().Evaluate(context)
            .Should().NotContain(item => item.Code == "STXD005");
    }

    [Fact]
    public void ExternalApiCall_WhenMadeFromODataBuilderExtension_IsNotReported()
    {
        EvaluationContext context = CreateExternalApiContext(
            source:
                "namespace Example.Exposures; "
                + "public static class ODataConventionModelBuilderExtensions "
                + "{ public static void Configure(this ThirdParty.ODataConventionModelBuilder builder) "
                + "{ builder.EntityType(); builder.EntitySet(); } }",
            externalSource:
                "namespace ThirdParty; public sealed class ODataConventionModelBuilder "
                + "{ public void EntityType() { } public void EntitySet() { } }",
            typeName:
                "Example.Exposures.ODataConventionModelBuilderExtensions");

        new STXDRulesProcessingService().Evaluate(context)
            .Should().NotContain(item => item.Code == "STXD005");
    }

    [Fact]
    public void ExternalBaseMethod_WhenCalledByExposure_IsNotReportedAsApiCall()
    {
        const string externalSource =
            "namespace ThirdParty; public abstract class ExternalController "
            + "{ protected string Ok() => string.Empty; }";

        const string source =
            "namespace Example.Controllers; "
            + "public sealed class ExampleController : ThirdParty.ExternalController "
            + "{ public string Get() => Ok(); }";

        EvaluationContext context = CreateExternalApiContext(
            source: source,
            externalSource: externalSource,
            typeName: "Example.Controllers.ExampleController");

        new STXDRulesProcessingService().Evaluate(context)
            .Should().NotContain(item => item.Code == "STXD005");
    }

    [Fact]
    public void ExternalAttribute_WhenAppliedToExposure_IsNotReportedAsApiCall()
    {
        const string externalSource =
            "namespace ThirdParty; public sealed class ExternalAttribute : System.Attribute { }";

        const string source =
            "namespace Example.Exposures; "
            + "[ThirdParty.External] public sealed class ExampleManager { }";

        EvaluationContext context = CreateExternalApiContext(
            source: source,
            externalSource: externalSource,
            typeName: "Example.Exposures.ExampleManager");

        new STXDRulesProcessingService().Evaluate(context)
            .Should().NotContain(item => item.Code == "STXD005");
    }

    private static void AssertExternalApiCallIsReported(
        string namespaceName,
        string typeName)
    {
        EvaluationContext context = CreateExternalApiContext(
            source:
                $"namespace {namespaceName}; "
                + $"public sealed class {typeName} "
                + "{ public string Execute() => ThirdParty.ExternalApi.Serialize(this); }",
            externalSource:
                "namespace ThirdParty; public static class ExternalApi "
                + "{ public static string Serialize(object value) => string.Empty; }",
            typeName: $"{namespaceName}.{typeName}");

        new STXDRulesProcessingService().Evaluate(context)
            .Should().ContainSingle(item => item.Code == "STXD005");
    }

    private static void AssertExternalApiCallIsNotReported(
        string namespaceName,
        string typeName)
    {
        EvaluationContext context = CreateExternalApiContext(
            source:
                $"namespace {namespaceName}; "
                + $"public sealed class {typeName} "
                + "{ public string Execute() => ThirdParty.ExternalApi.Serialize(this); }",
            externalSource:
                "namespace ThirdParty; public static class ExternalApi "
                + "{ public static string Serialize(object value) => string.Empty; }",
            typeName: $"{namespaceName}.{typeName}");

        new STXDRulesProcessingService().Evaluate(context)
            .Should().NotContain(item => item.Code == "STXD005");
    }

    private static EvaluationContext CreateExternalApiContext(
        string source,
        string externalSource,
        string typeName)
    {
        MetadataReference externalReference = CreateExternalReference(source: externalSource);
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(text: source, path: "Example.cs");
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Example",
            syntaxTrees: [syntaxTree],
            references: [.. GetPlatformReferences(), externalReference]);

        ArchitectureBuild build = new() { Compilation = compilation };
        Mock<IArchitectureService> architectureServiceMock = new();
        architectureServiceMock.Setup(service => service.Build(compilation)).Returns(build);

        ArchitectureBuild architectureBuild =
            new ArchitectureProcessingService(architectureServiceMock.Object)
                .Process(compilation);

        return new EvaluationContextsProcessingService()
            .Process(architectureBuild)
            .Single(context => context.ArchitectureElement.Name == typeName);
    }

    private static MetadataReference CreateExternalReference(string source)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "ThirdParty.External.Library",
            syntaxTrees: [CSharpSyntaxTree.ParseText(text: source)],
            references: GetPlatformReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using MemoryStream stream = new();
        EmitResult result = compilation.Emit(peStream: stream);
        result.Success.Should().BeTrue(
            string.Join(Environment.NewLine, result.Diagnostics));

        return MetadataReference.CreateFromImage(peImage: stream.ToArray());
    }

    private static MetadataReference[] GetPlatformReferences() =>
        ((string)AppContext.GetData(name: "TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(separator: Path.PathSeparator)
            .Select(selector: path => MetadataReference.CreateFromFile(path: path))
            .ToArray();
}