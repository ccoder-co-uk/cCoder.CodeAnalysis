// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXAPPRulesProcessingService : ISTXAPPRulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        foreach (AnalysisItem item in EvaluateSTXAPP001(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXAPP002(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXAPP003(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXAPP004(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXAPP006(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXAPP007(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXAPP008(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXAPP009(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXAPP010(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXAPP011(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXAPP012(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXAPP013(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXAPP014(context: context))
        {
            yield return item;
        }

        foreach (AnalysisItem item in EvaluateSTXAPP015(context: context))
        {
            yield return item;
        }
    }

    private static AnalysisItem CreateAnalysisItem(
        string code,
        string description,
        EvaluationContext context,
        Microsoft.CodeAnalysis.Location? location = null
    )
    {
        return new AnalysisItem
        {
            Code = code,
            Description = description,
            Severity = AnalysisSeverity.Warning,
            Type = context.TypeName,
            LineNumber = location is null ? context.LineNumber : location.GetLineSpan().StartLinePosition.Line + 1,
        };
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP001(EvaluationContext context)
    {
        string typeName = context.TypeName.Split(separator: ['.'])
            .Last();

        string[] filePathParts = context.FilePath.Replace(oldChar: '\\', newChar: '/')
            .Split(separator: ['/']);

        string fileName = filePathParts.LastOrDefault() ?? string.Empty;
        string parentFolder = filePathParts.Length > 1 ? filePathParts[filePathParts.Length - 2] : string.Empty;

        bool hasConventionalFileName =
            fileName == $"{typeName}.cs"
            || fileName.StartsWith(
                value: $"{typeName}.",
                comparisonType: StringComparison.Ordinal)
                && fileName.EndsWith(
                    value: ".cs",
                    comparisonType: StringComparison.Ordinal);

        bool livesAtProjectRoot =
            hasConventionalFileName
            && parentFolder.Equals(value: context.ProjectName, comparisonType: StringComparison.Ordinal);

        return livesAtProjectRoot
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXAPP001",
                    description: "Application composition helpers must live at the project root.",
                    context: context
                ),
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP002(EvaluationContext context)
    {
        if (context.TypeName.Split(separator: ['.'])
            .Last() != "IServiceCollectionExtensions")
        {
            return [];
        }

        bool isDomainLibrary = context.ProjectName.StartsWith(
            "cCoder.",
            StringComparison.OrdinalIgnoreCase);
        string supportingDataRegistration =
            GetSupportingDataRegistrationName(context.ProjectName);

        bool exposesDomainRegistration = GetMethods(context)
            .Any(method =>
                method.Modifiers.Any(modifier =>
                    modifier.RawKind == (int)
                        Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword)
                && method.ParameterList.Parameters.Any(parameter =>
                    parameter.Type?.ToString() == "IServiceCollection")
                && (!string.IsNullOrWhiteSpace(supportingDataRegistration)
                    ? method.Identifier.Text == supportingDataRegistration
                    : isDomainLibrary
                    ? method.Identifier.Text.StartsWith(
                        "Add",
                        StringComparison.Ordinal)
                        && (method.Identifier.Text.EndsWith(
                            "Web",
                            StringComparison.Ordinal)
                            || method.Identifier.Text.EndsWith(
                                "HostedServices",
                                StringComparison.Ordinal))
                    : IsApplicationEntryPoint(
                        method: method,
                        projectName: context.ProjectName)));

        return exposesDomainRegistration
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXAPP002",
                    description: "Libraries must expose Add{Domain}Web or Add{Domain}HostedServices, supporting data libraries expose Add{Domain}Data, and apps expose Add{AppName}.",
                    context: context
                ),
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP003(EvaluationContext context)
    {
        if (context.TypeName.Split(separator: ['.'])
            .Last() != "IServiceCollectionExtensions")
        {
            return [];
        }

        InvocationExpressionSyntax? invalidRegistration = context
            .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.DescendantNodes())
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault(
                predicate: (InvocationExpressionSyntax invocation) =>
                    invocation.ToString()
            .Contains(value: "Configuration", comparisonType: StringComparison.Ordinal)
                    && (
                        invocation
                            .Expression.ToString()
            .Contains(value: "AddScoped", comparisonType: StringComparison.Ordinal)
                        || invocation
                            .Expression.ToString()
            .Contains(value: "AddTransient", comparisonType: StringComparison.Ordinal)
                    )
            );

        return invalidRegistration is null
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXAPP003",
                    description: "Configuration objects should be registered as singletons.",
                    context: context,
                    location: invalidRegistration.GetLocation()
                ),
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP004(EvaluationContext context)
    {
        if (context.TypeName.Split(separator: ['.'])
            .Last() != "WebApplicationExtensions")
        {
            return [];
        }

        bool startsServicesThroughProvider = GetMethods(context: context)
            .Any(
                predicate: (MethodDeclarationSyntax method) =>
                    method.ParameterList.Parameters.Any(
                        predicate: (ParameterSyntax parameter) =>
                            parameter.Type?.ToString() is "IServiceProvider" or "WebApplication"
                    )
                    && method
                        .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
                        .Any(
                            predicate: (InvocationExpressionSyntax invocation) =>
                                invocation
                                    .Expression.ToString()
            .Contains(value: "GetRequiredService", comparisonType: StringComparison.Ordinal)
                                || invocation
                                    .Expression.ToString()
            .Contains(value: "GetService", comparisonType: StringComparison.Ordinal)
                        )
            );

        return startsServicesThroughProvider
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXAPP004",
                    description: "WebApplicationExtensions must consume the service provider to start application services.",
                    context: context
                ),
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP006(EvaluationContext context)
    {
        if (
            GetTypeName(context: context) != "Program"
            || !context.IsConsoleApplication
            || !IsCommandApplication(context: context)
        )
        {
            return [];
        }

        bool hasHostExtensions = context.ProjectTypeNames.Any(
            predicate: (string typeName) =>
                typeName.EndsWith(value: ".IHostExtensions", comparisonType: StringComparison.Ordinal)
        );

        return hasHostExtensions
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXAPP006",
                    description: "Console command applications must declare a root IHostExtensions composition class.",
                    context: context
                ),
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP007(EvaluationContext context)
    {
        if (GetTypeName(context: context) != "IHostExtensions")
        {
            return [];
        }

        bool routesCommandsThroughProvider = GetMethods(context: context)
            .Any(predicate: RoutesCommandThroughProvider);

        return routesCommandsThroughProvider
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXAPP007",
                    description: "IHostExtensions must route requested command details to a handling service resolved from the service provider.",
                    context: context
                ),
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP008(EvaluationContext context)
    {
        if (GetTypeName(context: context) != "IServiceCollectionExtensions")
        {
            return [];
        }

        InvocationExpressionSyntax? chainedRegistration = GetMethods(context: context)
            .SelectMany(selector: (MethodDeclarationSyntax method) => method.DescendantNodes())
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault(predicate: IsChainedServiceCollectionRegistration);

        return chainedRegistration is null
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXAPP008",
                    description: "IServiceCollection registrations must be declared as individual statements rather than fluent chains.",
                    context: context,
                    location: chainedRegistration.GetLocation()
                ),
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP009(EvaluationContext context)
    {
        if (GetTypeName(context: context) != "IServiceCollectionExtensions")
        {
            return [];
        }

        MethodDeclarationSyntax[] methods = GetMethods(context: context).ToArray();

        MethodDeclarationSyntax[] domainRegistrationMethods = methods
            .Where(predicate: IsDomainRegistrationMethod)
            .Where(
                predicate: (MethodDeclarationSyntax method) =>
                    method.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Any()
            )
            .ToArray();

        MethodDeclarationSyntax? invalidMethod = domainRegistrationMethods.FirstOrDefault(
            predicate: (MethodDeclarationSyntax method) =>
                !DelegatesRegistrationByLayer(method: method, methods: methods)
        );

        return invalidMethod is null
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXAPP009",
                    description: "Application registration must delegate app-owned services to private architectural-layer IServiceCollection extensions.",
                    context: context,
                    location: invalidMethod.GetLocation()
                ),
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP010(
        EvaluationContext context)
    {
        if (GetTypeName(context) != "IServiceCollectionExtensions")
        {
            return [];
        }

        MethodDeclarationSyntax? invalidMethod = GetMethods(context)
            .FirstOrDefault(method =>
                method.ParameterList.Parameters.FirstOrDefault()
                    is not ParameterSyntax parameter
                || parameter.Type?.ToString() != "IServiceCollection"
                || !parameter.Modifiers.Any(modifier =>
                    modifier.RawKind == (int)
                        Microsoft.CodeAnalysis.CSharp.SyntaxKind.ThisKeyword));

        return invalidMethod is null
            ? []
            :
            [
                CreateAnalysisItem(
                    "STXAPP010",
                    "IServiceCollectionExtensions may contain only IServiceCollection extension methods.",
                    context,
                    invalidMethod.GetLocation())
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP011(
        EvaluationContext context)
    {
        if (GetTypeName(context) != "IServiceCollectionExtensions")
        {
            return [];
        }

        if (context.ProjectName.StartsWith(
            "cCoder.",
            StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        MethodDeclarationSyntax? publicEntryPoint = GetMethods(context)
            .FirstOrDefault(method =>
                method.Modifiers.Any(modifier =>
                    modifier.RawKind == (int)
                        Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword)
                && IsApplicationEntryPoint(
                    method: method,
                    projectName: context.ProjectName));

        return publicEntryPoint is not null
            ? []
            :
            [
                CreateAnalysisItem(
                    "STXAPP011",
                    "Application IServiceCollectionExtensions must expose Add{RootConfigurationName}.",
                    context)
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP012(
        EvaluationContext context)
    {
        if (GetTypeName(context) != "IServiceCollectionExtensions")
        {
            return [];
        }

        if (context.ProjectName.StartsWith(
            "cCoder.",
            StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        MethodDeclarationSyntax? entryPoint = GetMethods(context)
            .FirstOrDefault(method =>
                method.Modifiers.Any(modifier =>
                    modifier.RawKind == (int)
                        Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword)
                && method.ParameterList.Parameters.Any(parameter =>
                    parameter.Type?.ToString() == "IConfiguration"));

        if (entryPoint is null)
        {
            return [];
        }

        bool acceptsApplicationConfiguration =
            entryPoint.ParameterList.Parameters.Any(parameter =>
                parameter.Type?.ToString() == "IConfiguration");

        bool exposesConfigurationCallback = entryPoint.ParameterList.Parameters
            .Any(parameter =>
                parameter.Type is GenericNameSyntax genericName
                && genericName.Identifier.Text == "Action"
                && genericName.TypeArgumentList.Arguments.Count == 1
                && genericName.TypeArgumentList.Arguments[0]
                    .ToString()
                    .EndsWith(
                        "Configuration",
                        StringComparison.Ordinal));

        return acceptsApplicationConfiguration
            && exposesConfigurationCallback
            ? []
            :
            [
                CreateAnalysisItem(
                    "STXAPP012",
                    "Application registration must accept IConfiguration, bind its root configuration, and expose an Action<TConfiguration> adjustment callback.",
                    context,
                    entryPoint.GetLocation())
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP013(
        EvaluationContext context)
    {
        if (!GetTypeName(context).EndsWith(
            "Configuration",
            StringComparison.Ordinal))
        {
            return [];
        }

        PropertyDeclarationSyntax? invalidProperty = context.Declarations
            .SelectMany(declaration => declaration.Members)
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(property =>
                !property.Modifiers.Any(modifier =>
                    modifier.RawKind == (int)
                        Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword)
                || property.AccessorList is null
                || !property.AccessorList.Accessors.Any(accessor =>
                    accessor.Keyword.RawKind == (int)
                        Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetKeyword)
                || !property.AccessorList.Accessors.Any(accessor =>
                    accessor.Keyword.RawKind == (int)
                        Microsoft.CodeAnalysis.CSharp.SyntaxKind.SetKeyword)
                || property.Type.ToString() == "dynamic"
                || property.Type.ToString().Contains(
                    "Dictionary",
                    StringComparison.Ordinal));

        return invalidProperty is null
            ? []
            :
            [
                CreateAnalysisItem(
                    "STXAPP013",
                    "Configuration properties must be public, strongly typed, and bindable with get and set accessors.",
                    context,
                    invalidProperty.GetLocation())
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP014(
        EvaluationContext context)
    {
        if (GetTypeName(context) != "Program"
            || context.IsConsoleApplication)
        {
            return [];
        }

        bool bindsConfigurationInProgram = context.SourceCode.Contains(
            ".Bind",
            StringComparison.Ordinal);

        bool passesApplicationConfiguration = context.SourceCode.Contains(
            ".Configuration",
            StringComparison.Ordinal)
            && context.SourceCode.Contains(
                ".Services.Add",
                StringComparison.Ordinal);

        return !bindsConfigurationInProgram
            && passesApplicationConfiguration
            ? []
            :
            [
                CreateAnalysisItem(
                    "STXAPP014",
                    "Program must pass IConfiguration to app registration; the app extension owns root configuration creation and binding.",
                    context)
            ];
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP015(
        EvaluationContext context)
    {
        string configurationName = GetTypeName(context);
        string projectConfigurationName = string.Concat(
            context.ProjectName.Split(
                ['.', '-'],
                StringSplitOptions.RemoveEmptyEntries))
            + "Configuration";

        if (!string.Equals(
            configurationName,
            projectConfigurationName,
            StringComparison.Ordinal))
        {
            return [];
        }

        PropertyDeclarationSyntax? scalarProperty = context.Declarations
            .SelectMany(declaration => declaration.Members)
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(property =>
                !property.Type.ToString().EndsWith(
                    "Configuration",
                    StringComparison.Ordinal));

        return scalarProperty is null
            ? []
            :
            [
                CreateAnalysisItem(
                    "STXAPP015",
                    "Application root configuration properties must be domain or complex configuration objects; scalar values belong to a domain.",
                    context,
                    scalarProperty.GetLocation())
            ];
    }

    private static bool IsCommandApplication(EvaluationContext context) =>

        context.SourceCode.Contains(value: "RootCommand", comparisonType: StringComparison.Ordinal)
        || context.SourceCode.Contains(value: "System.CommandLine", comparisonType: StringComparison.Ordinal)
        || context.SourceCode.Contains(value: ".InvokeAsync(args", comparisonType: StringComparison.Ordinal)
        || context.SourceCode.Contains(value: ".RunAsync(args", comparisonType: StringComparison.Ordinal);

    private static bool RoutesCommandThroughProvider(MethodDeclarationSyntax method)
    {
        ParameterSyntax? commandParameter = method.ParameterList.Parameters.FirstOrDefault(
            predicate: (ParameterSyntax parameter) =>
                parameter.Type?.ToString() is "string" or "string[]" or "IReadOnlyList<string>"
        );

        if (commandParameter is null)
        {
            return false;
        }

        InvocationExpressionSyntax[] invocations = method
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .ToArray();

        bool resolvesHandlingService = invocations.Any(
            predicate: (InvocationExpressionSyntax invocation) =>
                invocation.Expression.ToString()
                    .Contains(value: "GetRequiredService", comparisonType: StringComparison.Ordinal)
                || invocation.Expression.ToString()
                    .Contains(value: "GetService", comparisonType: StringComparison.Ordinal)
        );

        bool passesCommandDetails = invocations.Any(
            predicate: (InvocationExpressionSyntax invocation) =>
                invocation.ArgumentList.Arguments.Any(
                    predicate: (ArgumentSyntax argument) =>
                        argument.Expression.ToString() == commandParameter.Identifier.Text
                )
        );

        return resolvesHandlingService && passesCommandDetails;
    }

    private static bool IsChainedServiceCollectionRegistration(InvocationExpressionSyntax invocation)
    {
        if (
            invocation.Expression is not MemberAccessExpressionSyntax memberAccess
            || memberAccess.Expression is not InvocationExpressionSyntax previousInvocation
        )
        {
            return false;
        }

        return GetInvocationRoot(previousInvocation: previousInvocation) is IdentifierNameSyntax identifier
            && identifier.Identifier.Text == "services"
            && memberAccess.Name.Identifier.Text.StartsWith(value: "Add", comparisonType: StringComparison.Ordinal);
    }

    private static ExpressionSyntax GetInvocationRoot(InvocationExpressionSyntax previousInvocation)
    {
        ExpressionSyntax expression = previousInvocation.Expression;

        while (expression is MemberAccessExpressionSyntax memberAccess)
        {
            expression = memberAccess.Expression is InvocationExpressionSyntax nestedInvocation
                ? nestedInvocation.Expression
                : memberAccess.Expression;
        }

        return expression;
    }

    private static bool IsDomainRegistrationMethod(MethodDeclarationSyntax method) =>

        method.Modifiers.Any(
            predicate: (Microsoft.CodeAnalysis.SyntaxToken modifier) =>
                modifier.RawKind == (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword
        )
        && method.Identifier.Text.StartsWith(value: "Add", comparisonType: StringComparison.Ordinal)
        && method.TypeParameterList is null
        && !method.Identifier.Text.EndsWith(
            value: "Providers",
            comparisonType: StringComparison.Ordinal)
        && method.ParameterList.Parameters.Any(
            predicate: (ParameterSyntax parameter) => parameter.Type?.ToString() == "IServiceCollection"
        );

    private static bool IsApplicationEntryPoint(
        MethodDeclarationSyntax method,
        string projectName)
    {
        GenericNameSyntax? configurationCallback = method.ParameterList
            .Parameters
            .Select(parameter => parameter.Type)
            .OfType<GenericNameSyntax>()
            .FirstOrDefault(genericName =>
                genericName.Identifier.Text == "Action"
                && genericName.TypeArgumentList.Arguments.Count == 1);

        if (configurationCallback is null)
        {
            return method.Identifier.Text.StartsWith(
                "Add",
                StringComparison.Ordinal)
                && method.ParameterList.Parameters.Any(parameter =>
                    parameter.Type?.ToString() == "IConfiguration");
        }

        string configurationType = configurationCallback
            .TypeArgumentList.Arguments[0]
            .ToString();

        if (!configurationType.EndsWith(
            "Configuration",
            StringComparison.Ordinal))
        {
            return false;
        }

        string configurationPrefix = configurationType.Substring(
            startIndex: 0,
            length: configurationType.Length - "Configuration".Length);
        string applicationSuffix = projectName
            .Split(separator: ['.'])
            .Last();

        if (!configurationPrefix.EndsWith(
            applicationSuffix,
            StringComparison.OrdinalIgnoreCase))
        {
            configurationPrefix += applicationSuffix;
        }

        string expectedMethodName = $"Add{configurationPrefix}";

        return method.Identifier.Text == expectedMethodName
            && method.ParameterList.Parameters.Any(parameter =>
                parameter.Type?.ToString() == "IConfiguration");
    }

    private static bool DelegatesRegistrationByLayer(
        MethodDeclarationSyntax method,
        IReadOnlyCollection<MethodDeclarationSyntax> methods
    )
    {
        string[] layerNames =
        [
            "Dependencies",
            "Brokers",
            "Foundations",
            "Processings",
            "Orchestrations",
            "Coordinations",
            "Managements",
            "Aggregations",
            "Exposures",
        ];

        MethodDeclarationSyntax[] layerInitializers = methods
            .Where(
                predicate: (MethodDeclarationSyntax candidate) =>
                    candidate.Modifiers.Any(modifier =>
                        modifier.RawKind == (int)
                            Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)
                    && layerNames.Any(
                        predicate: (string layerName) =>
                            candidate.Identifier.Text == $"Add{layerName}")
                    && candidate.ParameterList.Parameters.FirstOrDefault()
                        is ParameterSyntax parameter
                    && parameter.Type?.ToString() == "IServiceCollection"
                    && parameter.Modifiers.Any(modifier =>
                        modifier.RawKind == (int)
                            Microsoft.CodeAnalysis.CSharp.SyntaxKind.ThisKeyword)
            )
            .ToArray();

        if (layerInitializers.Length == 0)
        {
            return method.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Select(invocation => invocation.Expression)
                .OfType<MemberAccessExpressionSyntax>()
                .Select(memberAccess => memberAccess.Name.Identifier.Text)
                .Any(methodName =>
                    methodName.EndsWith(
                        "Web",
                        StringComparison.Ordinal)
                    || methodName.EndsWith(
                        "HostedServices",
                        StringComparison.Ordinal));
        }

        bool delegatesToRegistrationOverload = method
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Select(invocation => invocation.Expression)
            .OfType<MemberAccessExpressionSyntax>()
            .Any(memberAccess =>
                memberAccess.Name.Identifier.Text ==
                    method.Identifier.Text);

        if (delegatesToRegistrationOverload)
        {
            return true;
        }

        HashSet<string> invokedMethods = new HashSet<string>(
            collection: method
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Select(selector: (InvocationExpressionSyntax invocation) => invocation.Expression)
                .Select(
                    selector: (ExpressionSyntax expression) =>
                        expression switch
                        {
                            IdentifierNameSyntax identifier => identifier.Identifier.Text,
                            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
                            _ => string.Empty,
                        }
                ),
            comparer: StringComparer.Ordinal
        );

        return layerInitializers.All(initializer =>
            invokedMethods.Contains(initializer.Identifier.Text));
    }

    private static string GetTypeName(EvaluationContext context) =>

        context.TypeName.Split(separator: ['.'])
            .Last();

    private static string GetSupportingDataRegistrationName(
        string projectName)
    {
        string[] segments = projectName.Split(
            separator: ['.', '-'],
            options: StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 2
            || !string.Equals(
                segments[segments.Length - 1],
                "Data",
                StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return segments.Length == 2
            ? "AddData"
            : $"Add{segments[segments.Length - 2]}Data";
    }

    private static IEnumerable<MethodDeclarationSyntax> GetMethods(EvaluationContext context) =>

        context
            .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>();
}
