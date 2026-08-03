// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXAPPRulesProcessingService : ISTXAPPRulesProcessingService
{
    private static readonly IArchitectureModelQueriesProcessingService architectureModelQueries =
        new ArchitectureModelQueriesProcessingService();

    private static readonly string[] layerNames =
    [
        "Dependencies", "Brokers", "Foundations", "Processings", "Orchestrations",
        "Coordinations", "Managements", "Aggregations", "Exposures",
    ];

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        if (context.ArchitectureElement?.AnalysisTypeFacts is null)
        {
            return [];
        }

        return EvaluateSTXAPP001(context: context)
            .Concat(second: EvaluateSTXAPP002(context: context))
            .Concat(second: EvaluateSTXAPP003(context: context))
            .Concat(second: EvaluateSTXAPP004(context: context))
            .Concat(second: EvaluateSTXAPP006(context: context))
            .Concat(second: EvaluateSTXAPP007(context: context))
            .Concat(second: EvaluateSTXAPP008(context: context))
            .Concat(second: EvaluateSTXAPP009(context: context))
            .Concat(second: EvaluateSTXAPP010(context: context))
            .Concat(second: EvaluateSTXAPP011(context: context))
            .Concat(second: EvaluateSTXAPP012(context: context))
            .Concat(second: EvaluateSTXAPP013(context: context))
            .Concat(second: EvaluateSTXAPP014(context: context))
            .Concat(second: EvaluateSTXAPP015(context: context));
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP001(EvaluationContext context)
    {
        TypeAnalysisFacts facts = GetFacts(context);

        return IsApplicationElement(context)
            && !LivesAtProjectRoot(GetTypeName(context), facts.ProjectName, facts.FilePath)
                ? [Create("STXAPP001", "Application composition helpers must live at the project root.", context)]
                : [];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP002(EvaluationContext context)
    {
        TypeAnalysisFacts facts = GetFacts(context);

        return IsServiceCollectionExtensions(context)
            && !ExposesDomainRegistration(facts.Methods, facts.ProjectName)
                ? [Create("STXAPP002", "Libraries must expose Add{Domain}Web or Add{Domain}HostedServices, provider libraries expose Add{Domain}Providers, supporting data libraries expose Add{Domain}Data, and apps expose Add{AppName}.", context)]
                : [];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP003(EvaluationContext context)
    {
        MethodAnalysisFacts? method = IsServiceCollectionExtensions(context)
            ? GetFacts(context).Methods.FirstOrDefault(candidate =>
                candidate.HasScopedOrTransientConfigurationRegistration)
            : null;

        return method is null ? [] : [Create("STXAPP003", "Configuration objects should be registered as singletons.", context, method.LineNumber)];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP004(EvaluationContext context)
    {
        TypeAnalysisFacts facts = GetFacts(context);

        return IsApplicationElement(context)
            && GetTypeName(context) == "WebApplicationExtensions"
            && !facts.Methods.Any(method => method.ResolvesServiceFromProvider)
                ? [Create("STXAPP004", "WebApplicationExtensions must consume the service provider to start application services.", context)]
                : [];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP006(EvaluationContext context)
    {
        TypeAnalysisFacts facts = GetFacts(context);

        return IsApplicationElement(context)
            && GetTypeName(context) == "Program"
            && IsCommandApplication(facts.SourceCode)
            && !facts.ProjectTypeNames.Any(name => name.EndsWith(".IHostExtensions", StringComparison.Ordinal))
                ? [Create("STXAPP006", "Console command applications must declare a root IHostExtensions composition class.", context)]
                : [];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP007(EvaluationContext context)
    {
        TypeAnalysisFacts facts = GetFacts(context);

        return IsApplicationElement(context)
            && GetTypeName(context) == "IHostExtensions"
            && !facts.Methods.Any(method => method.HasCommandDetailsParameter
                && method.ResolvesServiceFromProvider
                && method.PassesCommandDetails)
                ? [Create("STXAPP007", "IHostExtensions must route requested command details to a handling service resolved from the service provider.", context)]
                : [];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP008(EvaluationContext context)
    {
        MethodAnalysisFacts? method = IsServiceCollectionExtensions(context)
            ? GetFacts(context).Methods.FirstOrDefault(candidate => candidate.HasChainedServiceCollectionRegistration)
            : null;

        return method is null ? [] : [Create("STXAPP008", "IServiceCollection registrations must be declared as individual statements rather than fluent chains.", context, method.LineNumber)];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP009(EvaluationContext context)
    {
        if (!IsServiceCollectionExtensions(context)) return [];

        MethodAnalysisFacts[] methods = GetFacts(context).Methods.ToArray();

        MethodAnalysisFacts? method = methods.Where(IsDomainRegistrationMethod)
            .Where(candidate => candidate.HasInvocations)
            .FirstOrDefault(candidate => !DelegatesRegistrationByLayer(candidate, methods));

        return method is null ? [] : [Create("STXAPP009", "Application registration must delegate app-owned services to private architectural-layer IServiceCollection extensions.", context, method.LineNumber)];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP010(EvaluationContext context)
    {
        MethodAnalysisFacts? method = IsServiceCollectionExtensions(context)
            ? GetFacts(context).Methods.FirstOrDefault(candidate => !candidate.FirstParameterIsServiceCollectionExtension)
            : null;

        return method is null ? [] : [Create("STXAPP010", "IServiceCollectionExtensions may contain only IServiceCollection extension methods.", context, method.LineNumber)];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP011(EvaluationContext context)
    {
        TypeAnalysisFacts facts = GetFacts(context);

        bool applies = IsServiceCollectionExtensions(context)
            && !facts.ProjectName.StartsWith("cCoder.", StringComparison.OrdinalIgnoreCase);

        bool hasEntryPoint = facts.Methods.Any(method =>
            method.IsPublic && IsApplicationEntryPoint(method, facts.ProjectName));

        return applies && !hasEntryPoint
            ? [Create("STXAPP011", "Application IServiceCollectionExtensions must expose Add{AppName}.", context)]
            : [];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP012(EvaluationContext context)
    {
        TypeAnalysisFacts facts = GetFacts(context);

        if (!IsServiceCollectionExtensions(context)
            || facts.ProjectName.StartsWith("cCoder.", StringComparison.OrdinalIgnoreCase)) return [];

        MethodAnalysisFacts? method = facts.Methods.FirstOrDefault(candidate =>
            candidate.IsPublic && candidate.HasConfigurationParameter);

        return method is not null && string.IsNullOrWhiteSpace(method.ConfigurationCallbackType)
            ? [Create("STXAPP012", "Application registration must accept IConfiguration, bind its root configuration, and expose an Action<TConfiguration> adjustment callback.", context, method.LineNumber)]
            : [];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP013(EvaluationContext context)
    {
        if (!GetTypeName(context).EndsWith("Configuration", StringComparison.Ordinal)) return [];

        PropertyAnalysisFacts? property = GetFacts(context).Properties.FirstOrDefault(candidate =>
            !candidate.IsPublic || !candidate.HasGetter || !candidate.HasSetter
            || candidate.TypeName == "dynamic"
            || IsDisallowedConfigurationDictionary(candidate.TypeName));

        return property is null ? [] : [Create("STXAPP013", "Configuration properties must be public, strongly typed, and bindable with get and set accessors.", context, property.LineNumber)];
    }

    private static bool IsDisallowedConfigurationDictionary(string typeName)
    {
        if (!typeName.Contains("Dictionary", StringComparison.Ordinal))
        {
            return false;
        }

        string compactTypeName = typeName.Replace(" ", string.Empty);

        string[] supportedPrefixes =
        [
            "Dictionary<string,",
            "IReadOnlyDictionary<string,",
            "System.Collections.Generic.Dictionary<string,",
            "System.Collections.Generic.IReadOnlyDictionary<string,",
        ];

        string? prefix = supportedPrefixes.FirstOrDefault(candidate =>
            compactTypeName.StartsWith(candidate, StringComparison.Ordinal));

        if (prefix is null || !compactTypeName.EndsWith(">", StringComparison.Ordinal))
        {
            return true;
        }

        string valueTypeName = compactTypeName.Substring(
            startIndex: prefix.Length,
            length: compactTypeName.Length - prefix.Length - 1).TrimEnd('?');

        if (valueTypeName.IndexOf('<') >= 0)
        {
            return true;
        }

        string simpleValueTypeName = valueTypeName.Split('.').Last();

        bool looksLikeInterface = simpleValueTypeName.Length > 1
            && simpleValueTypeName[0] == 'I'
            && char.IsUpper(simpleValueTypeName[1]);

        return looksLikeInterface
            || !simpleValueTypeName.EndsWith("Configuration", StringComparison.Ordinal);
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP014(EvaluationContext context)
    {
        TypeAnalysisFacts facts = GetFacts(context);

        if (!IsApplicationElement(context)
            || GetTypeName(context) != "Program"
            || facts.IsConsoleApplication
            || IsCommandApplication(facts.SourceCode)) return [];

        bool binds = facts.SourceCode.Contains(".Bind", StringComparison.Ordinal);

        bool passes = facts.SourceCode.Contains(".Configuration", StringComparison.Ordinal)
            && facts.SourceCode.Contains(".Services.Add", StringComparison.Ordinal);

        return binds || !passes
            ? [Create("STXAPP014", "Program must pass IConfiguration to app registration; the app extension owns root configuration creation and binding.", context)]
            : [];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP015(EvaluationContext context)
    {
        TypeAnalysisFacts facts = GetFacts(context);

        string expectedName = string.Concat(facts.ProjectName.Split(
            ['.', '-'], StringSplitOptions.RemoveEmptyEntries)) + "Configuration";

        if (GetTypeName(context) != expectedName) return [];

        PropertyAnalysisFacts? property = facts.Properties.FirstOrDefault(candidate =>
            !candidate.TypeName.EndsWith("Configuration", StringComparison.Ordinal));

        return property is null ? [] : [Create("STXAPP015", "Application root configuration properties must be domain or complex configuration objects; scalar values belong to a domain.", context, property.LineNumber)];
    }

    private static TypeAnalysisFacts GetFacts(EvaluationContext context) =>
        context.ArchitectureElement!.AnalysisTypeFacts;

    private static bool IsApplicationElement(EvaluationContext context) =>
        architectureModelQueries.GetStandardElementType(context) == StandardElementType.App;

    private static bool IsServiceCollectionExtensions(EvaluationContext context) =>
        IsApplicationElement(context)
        && GetTypeName(context) == "IServiceCollectionExtensions";

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

    private static string GetTypeName(EvaluationContext context) =>
        architectureModelQueries.GetTypeName(context).Split('.').Last();

    private static AnalysisItem Create(
        string code,
        string description,
        EvaluationContext context,
        int lineNumber = 0) => new()
        {
            Code = code,
            Description = description,
            Severity = AnalysisSeverity.Warning,
            Type = architectureModelQueries.GetTypeName(context),
            LineNumber = lineNumber == 0
                ? architectureModelQueries.GetLineNumber(context)
                : lineNumber,
        };
}
