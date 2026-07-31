// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXAPPRulesProcessingService : ISTXAPPRulesProcessingService
{
    private static readonly string[] layerNames =
    [
        "Dependencies", "Brokers", "Foundations", "Processings", "Orchestrations",
        "Coordinations", "Managements", "Aggregations", "Exposures",
    ];

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        TypeAnalysisFacts? facts = context.ArchitectureElement?.AnalysisTypeFacts;

        if (facts is null)
        {
            yield break;
        }

        string typeName = GetTypeName(context.TypeName);
        string projectName = facts.ProjectName;
        bool isApplicationElement = context.StandardElementType == StandardElementType.App;

        if (isApplicationElement
            && !LivesAtProjectRoot(typeName, projectName, facts.FilePath))
        {
            yield return Create("STXAPP001", "Application composition helpers must live at the project root.", context);
        }

        if (isApplicationElement && typeName == "IServiceCollectionExtensions")
        {
            if (!ExposesDomainRegistration(facts.Methods, projectName))
            {
                yield return Create("STXAPP002", "Libraries must expose Add{Domain}Web or Add{Domain}HostedServices, provider libraries expose Add{Domain}Providers, supporting data libraries expose Add{Domain}Data, and apps expose Add{AppName}.", context);
            }

            MethodAnalysisFacts? invalidLifetime = facts.Methods.FirstOrDefault(
                method => method.HasScopedOrTransientConfigurationRegistration);

            if (invalidLifetime is not null)
            {
                yield return Create("STXAPP003", "Configuration objects should be registered as singletons.", context, invalidLifetime.LineNumber);
            }

            MethodAnalysisFacts? chained = facts.Methods.FirstOrDefault(
                method => method.HasChainedServiceCollectionRegistration);

            if (chained is not null)
            {
                yield return Create("STXAPP008", "IServiceCollection registrations must be declared as individual statements rather than fluent chains.", context, chained.LineNumber);
            }

            MethodAnalysisFacts[] methods = facts.Methods.ToArray();
            MethodAnalysisFacts? invalidLayering = methods
                .Where(IsDomainRegistrationMethod)
                .Where(method => method.HasInvocations)
                .FirstOrDefault(method => !DelegatesRegistrationByLayer(method, methods));

            if (invalidLayering is not null)
            {
                yield return Create("STXAPP009", "Application registration must delegate app-owned services to private architectural-layer IServiceCollection extensions.", context, invalidLayering.LineNumber);
            }

            MethodAnalysisFacts? nonExtension = methods.FirstOrDefault(
                method => !method.FirstParameterIsServiceCollectionExtension);

            if (nonExtension is not null)
            {
                yield return Create("STXAPP010", "IServiceCollectionExtensions may contain only IServiceCollection extension methods.", context, nonExtension.LineNumber);
            }

            if (!projectName.StartsWith("cCoder.", StringComparison.OrdinalIgnoreCase))
            {
                MethodAnalysisFacts? entryPoint = methods.FirstOrDefault(
                    method => method.IsPublic && IsApplicationEntryPoint(method, projectName));

                if (entryPoint is null)
                {
                    yield return Create("STXAPP011", "Application IServiceCollectionExtensions must expose Add{AppName}.", context);
                }

                MethodAnalysisFacts? configurationEntryPoint = methods.FirstOrDefault(
                    method => method.IsPublic && method.HasConfigurationParameter);

                if (configurationEntryPoint is not null
                    && string.IsNullOrWhiteSpace(configurationEntryPoint.ConfigurationCallbackType))
                {
                    yield return Create("STXAPP012", "Application registration must accept IConfiguration, bind its root configuration, and expose an Action<TConfiguration> adjustment callback.", context, configurationEntryPoint.LineNumber);
                }
            }
        }

        if (isApplicationElement && typeName == "WebApplicationExtensions"
            && !facts.Methods.Any(method => method.ResolvesServiceFromProvider))
        {
            yield return Create("STXAPP004", "WebApplicationExtensions must consume the service provider to start application services.", context);
        }

        if (isApplicationElement && typeName == "Program"
            && IsCommandApplication(facts.SourceCode)
            && !facts.ProjectTypeNames.Any(name => name.EndsWith(".IHostExtensions", StringComparison.Ordinal)))
        {
            yield return Create("STXAPP006", "Console command applications must declare a root IHostExtensions composition class.", context);
        }

        if (isApplicationElement && typeName == "IHostExtensions"
            && !facts.Methods.Any(method => method.HasCommandDetailsParameter
                && method.ResolvesServiceFromProvider
                && method.PassesCommandDetails))
        {
            yield return Create("STXAPP007", "IHostExtensions must route requested command details to a handling service resolved from the service provider.", context);
        }

        if (typeName.EndsWith("Configuration", StringComparison.Ordinal))
        {
            PropertyAnalysisFacts? invalidProperty = facts.Properties.FirstOrDefault(property =>
                !property.IsPublic || !property.HasGetter || !property.HasSetter
                || property.TypeName == "dynamic"
                || property.TypeName.Contains("Dictionary", StringComparison.Ordinal));

            if (invalidProperty is not null)
            {
                yield return Create("STXAPP013", "Configuration properties must be public, strongly typed, and bindable with get and set accessors.", context, invalidProperty.LineNumber);
            }
        }

        if (isApplicationElement
            && typeName == "Program"
            && !facts.IsConsoleApplication
            && !IsCommandApplication(facts.SourceCode))
        {
            bool bindsConfiguration = facts.SourceCode.Contains(".Bind", StringComparison.Ordinal);
            bool passesConfiguration = facts.SourceCode.Contains(".Configuration", StringComparison.Ordinal)
                && facts.SourceCode.Contains(".Services.Add", StringComparison.Ordinal);

            if (bindsConfiguration || !passesConfiguration)
            {
                yield return Create("STXAPP014", "Program must pass IConfiguration to app registration; the app extension owns root configuration creation and binding.", context);
            }
        }

        string projectConfigurationName = string.Concat(projectName.Split(
            ['.', '-'], StringSplitOptions.RemoveEmptyEntries)) + "Configuration";

        if (typeName == projectConfigurationName)
        {
            PropertyAnalysisFacts? scalar = facts.Properties.FirstOrDefault(
                property => !property.TypeName.EndsWith("Configuration", StringComparison.Ordinal));

            if (scalar is not null)
            {
                yield return Create("STXAPP015", "Application root configuration properties must be domain or complex configuration objects; scalar values belong to a domain.", context, scalar.LineNumber);
            }
        }
    }

    private static bool LivesAtProjectRoot(string typeName, string projectName, string filePath)
    {
        string[] parts = filePath.Replace('\\', '/').Split('/');
        string fileName = parts.LastOrDefault() ?? string.Empty;
        string parent = parts.Length > 1 ? parts[parts.Length - 2] : string.Empty;
        bool conventional = fileName == $"{typeName}.cs"
            || fileName.StartsWith($"{typeName}.", StringComparison.Ordinal)
                && fileName.EndsWith(".cs", StringComparison.Ordinal);

        return conventional && parent.Equals(projectName, StringComparison.Ordinal);
    }

    private static bool ExposesDomainRegistration(
        IReadOnlyList<MethodAnalysisFacts> methods,
        string projectName)
    {
        string supportingData = GetSupportingDataRegistrationName(projectName);
        string provider = GetProviderRegistrationName(projectName);
        bool isDomainLibrary = projectName.StartsWith("cCoder.", StringComparison.OrdinalIgnoreCase);

        return methods.Any(method => method.IsPublic
            && method.HasServiceCollectionParameter
            && (!string.IsNullOrWhiteSpace(provider)
                ? method.Name == provider
                : !string.IsNullOrWhiteSpace(supportingData)
                    ? method.Name == supportingData
                    : isDomainLibrary
                        ? method.Name.StartsWith("Add", StringComparison.Ordinal)
                            && (method.Name.EndsWith("Web", StringComparison.Ordinal)
                                || method.Name.EndsWith("HostedServices", StringComparison.Ordinal))
                        : IsApplicationEntryPoint(method, projectName)));
    }

    private static bool IsDomainRegistrationMethod(MethodAnalysisFacts method) =>
        method.IsPublic
        && method.Name.StartsWith("Add", StringComparison.Ordinal)
        && !method.IsGeneric
        && !method.Name.EndsWith("Providers", StringComparison.Ordinal)
        && method.HasServiceCollectionParameter;

    private static bool IsApplicationEntryPoint(
        MethodAnalysisFacts method,
        string projectName)
    {
        if (string.IsNullOrWhiteSpace(method.ConfigurationCallbackType))
        {
            return method.Name.StartsWith("Add", StringComparison.Ordinal)
                && method.HasConfigurationParameter;
        }

        string callbackType = method.ConfigurationCallbackType;

        if (!callbackType.EndsWith("Configuration", StringComparison.Ordinal))
        {
            return false;
        }

        string prefix = callbackType.Substring(
            startIndex: 0,
            length: callbackType.Length - "Configuration".Length);
        string suffix = projectName.Split('.').Last();

        if (!prefix.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            prefix += suffix;
        }

        return (method.Name == $"Add{prefix}" || method.Name == $"Add{suffix}")
            && method.HasConfigurationParameter;
    }

    private static bool DelegatesRegistrationByLayer(
        MethodAnalysisFacts method,
        IReadOnlyCollection<MethodAnalysisFacts> methods)
    {
        MethodAnalysisFacts[] initializers = methods.Where(candidate =>
            candidate.IsPrivate
            && layerNames.Any(layer => candidate.Name == $"Add{layer}")
            && candidate.FirstParameterIsServiceCollectionExtension).ToArray();

        if (initializers.Length == 0)
        {
            return method.InvokedMethodNames.Any(name =>
                name.EndsWith("Web", StringComparison.Ordinal)
                || name.EndsWith("HostedServices", StringComparison.Ordinal));
        }

        return method.InvokedMethodNames.Contains(method.Name, StringComparer.Ordinal)
            || initializers.All(initializer =>
                method.InvokedMethodNames.Contains(initializer.Name, StringComparer.Ordinal));
    }

    private static bool IsCommandApplication(string sourceCode) =>
        sourceCode.Contains("RootCommand", StringComparison.Ordinal)
        || sourceCode.Contains("System.CommandLine", StringComparison.Ordinal)
        || sourceCode.Contains(".InvokeAsync(args", StringComparison.Ordinal)
        || sourceCode.Contains(".RunAsync(args", StringComparison.Ordinal);

    private static string GetProviderRegistrationName(string projectName)
    {
        string[] segments = projectName.Split('.');
        return segments.Length >= 3 && segments[segments.Length - 1].Equals("Providers", StringComparison.OrdinalIgnoreCase)
            ? $"Add{segments[segments.Length - 2]}Providers"
            : string.Empty;
    }

    private static string GetSupportingDataRegistrationName(string projectName)
    {
        string[] segments = projectName.Split(['.', '-'], StringSplitOptions.RemoveEmptyEntries);
        return segments.Length < 2 || !segments[segments.Length - 1].Equals("Data", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : segments.Length == 2 ? "AddData" : $"Add{segments[segments.Length - 2]}Data";
    }

    private static string GetTypeName(string fullName) => fullName.Split('.').Last();

    private static AnalysisItem Create(
        string code,
        string description,
        EvaluationContext context,
        int lineNumber = 0) => new()
        {
            Code = code,
            Description = description,
            Severity = AnalysisSeverity.Warning,
            Type = context.TypeName,
            LineNumber = lineNumber == 0 ? context.LineNumber : lineNumber,
        };
}
