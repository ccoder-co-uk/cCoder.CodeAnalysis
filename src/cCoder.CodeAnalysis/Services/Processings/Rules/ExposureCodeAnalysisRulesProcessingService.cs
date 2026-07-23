// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class ExposureCodeAnalysisRulesProcessingService
    : CodeAnalysisRulesProcessingService,
        IExposureCodeAnalysisRulesProcessingService
{
    public AnalysisItem[] Evaluate(EvaluationContext context)
    {
        if (context.StandardElementType == StandardElementType.App)
        {
            return EvaluateAppRules(context);
        }

        List<AnalysisItem> list = new List<AnalysisItem>();
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateSourceFormatting(context));
        list.AddRange(ExposureCodeAnalysisRulesProcessingService.EvaluateSTXE001(context));
        list.AddRange(ExposureCodeAnalysisRulesProcessingService.EvaluateSTXE002(context));
        list.AddRange(ExposureCodeAnalysisRulesProcessingService.EvaluateSTXE003(context));
        list.AddRange(ExposureCodeAnalysisRulesProcessingService.EvaluateSTXE004(context));
        list.AddRange(ExposureCodeAnalysisRulesProcessingService.EvaluateSTXE005(context));
        list.AddRange(ExposureCodeAnalysisRulesProcessingService.EvaluateSTXAPI001(context));
        list.AddRange(ExposureCodeAnalysisRulesProcessingService.EvaluateSTXAPI002(context));
        list.AddRange(ExposureCodeAnalysisRulesProcessingService.EvaluateSTXAPI003(context));
        list.AddRange(ExposureCodeAnalysisRulesProcessingService.EvaluateSTXAPI004(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluatePropertiesAreNotAllowed(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateTypedIdentifiers(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateCreationReturnTypeNaming(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateMutationNaming(context));
        return list.ToArray();
    }

    private static AnalysisItem[] EvaluateAppRules(EvaluationContext context)
    {
        List<AnalysisItem> items = new List<AnalysisItem>();
        string typeName = context.TypeName.Split('.').Last();
        TypeDeclarationSyntax? declaration = context.Declarations.FirstOrDefault();
        string filePath = context.FilePath.Replace('\\', '/');
        string[] filePathParts = filePath.Split('/');
        string fileName = filePathParts.LastOrDefault() ?? string.Empty;
        string parentFolder = filePathParts.Length > 1 ? filePathParts[filePathParts.Length - 2] : string.Empty;
        bool livesAtProjectRoot = fileName == $"{typeName}.cs"
            && parentFolder.Equals(context.ProjectName, StringComparison.Ordinal);

        if (!livesAtProjectRoot)
        {
            items.Add(
                CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXAPP001",
                    "Program.cs, IServiceCollectionExtensions.cs, and WebApplicationExtensions.cs must live at the project root.",
                    context
                )
            );
        }

        if (typeName == "IServiceCollectionExtensions")
        {
            MethodDeclarationSyntax[] methods = context.Declarations
                .SelectMany((TypeDeclarationSyntax item) => item.Members)
                .OfType<MethodDeclarationSyntax>()
                .ToArray();
            bool exposesDomainRegistration = methods.Any(
                (MethodDeclarationSyntax method) =>
                    method.Identifier.Text.StartsWith("Add", StringComparison.Ordinal)
                    && !method.Identifier.Text.EndsWith("HostedServices", StringComparison.Ordinal)
                    && method.ParameterList.Parameters.Any(
                        (ParameterSyntax parameter) => parameter.Type?.ToString() == "IServiceCollection"
                    )
            );

            if (!exposesDomainRegistration)
            {
                items.Add(
                    CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                        "STXAPP002",
                        "IServiceCollectionExtensions must expose an Add{Domain} IServiceCollection extension.",
                        context
                    )
                );
            }

            InvocationExpressionSyntax? invalidConfigurationRegistration = context.Declarations
                .SelectMany((TypeDeclarationSyntax item) => item.DescendantNodes())
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault(
                    (InvocationExpressionSyntax invocation) =>
                        invocation.ToString().Contains("Configuration", StringComparison.Ordinal)
                        && (
                            invocation.Expression.ToString().Contains("AddScoped", StringComparison.Ordinal)
                            || invocation.Expression.ToString().Contains("AddTransient", StringComparison.Ordinal)
                        )
                );

            if (invalidConfigurationRegistration is not null)
            {
                items.Add(
                    CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                        "STXAPP003",
                        "Configuration objects should be registered as singletons.",
                        context,
                        invalidConfigurationRegistration.GetLocation()
                    )
                );
            }
        }

        if (typeName == "Program")
        {
            string[] applicationNamespaces = context.UsingNamespaces
                .Where(
                    (string item) =>
                        !item.Equals("System", StringComparison.Ordinal)
                        && !item.StartsWith("System.", StringComparison.Ordinal)
                        && !item.Equals("Microsoft", StringComparison.Ordinal)
                        && !item.StartsWith("Microsoft.", StringComparison.Ordinal)
                )
                .ToArray();

            bool usesServiceCollection =
                context.SourceCode.Contains(".Services", StringComparison.Ordinal)
                && context.SourceCode.Contains(".Add", StringComparison.Ordinal);

            if (applicationNamespaces.Length > 1 || !usesServiceCollection)
            {
                items.Add(
                    CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                        "STXAPP005",
                        "Program.cs must compose the app through IServiceCollection using SDK namespaces and one local composition namespace only.",
                        context
                    )
                );
            }
        }

        if (typeName == "WebApplicationExtensions")
        {
            bool startsServicesThroughProvider = context.Declarations
                .SelectMany((TypeDeclarationSyntax item) => item.Members)
                .OfType<MethodDeclarationSyntax>()
                .Any(
                    (MethodDeclarationSyntax method) =>
                        method.ParameterList.Parameters.Any(
                            (ParameterSyntax parameter) =>
                                parameter.Type?.ToString() is "IServiceProvider" or "WebApplication"
                        )
                        && method
                            .DescendantNodes()
                            .OfType<InvocationExpressionSyntax>()
                            .Any(
                                (InvocationExpressionSyntax invocation) =>
                                    invocation.Expression.ToString().Contains("GetRequiredService", StringComparison.Ordinal)
                                    || invocation.Expression.ToString().Contains("GetService", StringComparison.Ordinal)
                            )
                );

            if (!startsServicesThroughProvider)
            {
                items.Add(
                    CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                        "STXAPP004",
                        "WebApplicationExtensions must consume the service provider to start application services.",
                        context
                    )
                );
            }
        }

        return items.ToArray();
    }

    private static AnalysisItem[] EvaluateSTXE001(EvaluationContext context)
    {
        return context.IsApiController
            || context.TypeName.Split('.').Last() == "Program"
            ? Array.Empty<AnalysisItem>()
            : (
                from node in context.Declarations.SelectMany(
                    (TypeDeclarationSyntax declaration) => declaration.DescendantNodes()
                )
                where
                    (node is IfStatementSyntax || node is SwitchStatementSyntax || node is ConditionalExpressionSyntax)
                        ? true
                        : false
                select CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXE001",
                    "An exposure must not contain branching logic.",
                    context,
                    node.GetLocation()
                )
            ).ToArray();
    }

    private static AnalysisItem[] EvaluateSTXE002(EvaluationContext context)
    {
        return (
            from node in context.Declarations.SelectMany(
                (TypeDeclarationSyntax declaration) => declaration.DescendantNodes()
            )
            where
                (
                    node is ForStatementSyntax
                    || node is ForEachStatementSyntax
                    || node is WhileStatementSyntax
                    || node is DoStatementSyntax
                )
                    ? true
                    : false
            select CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                "STXE002",
                "An exposure must not contain loops.",
                context,
                node.GetLocation()
            )
        ).ToArray();
    }

    private static AnalysisItem[] EvaluateSTXE003(EvaluationContext context)
    {
        if (context.IsApiController)
        {
            return Array.Empty<AnalysisItem>();
        }
        int serviceDependencyCount = context.Dependencies.Count(
            delegate(TypeDependency dependency)
            {
                StandardElementType standardElementType = dependency.StandardElementType;

                return (uint)(standardElementType - 1) <= 6u;
            }
        );
        return (serviceDependencyCount <= 1)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXE003",
                    "An exposure may communicate with only one business service.",
                    context
                ),
            };
    }

    private static AnalysisItem[] EvaluateSTXE004(EvaluationContext context)
    {
        return (
            !context.Dependencies.Any(
                (TypeDependency dependency) => dependency.StandardElementType == StandardElementType.Broker
            )
        )
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXE004",
                    "An exposure must not communicate directly with a broker.",
                    context
                ),
            };
    }

    private static AnalysisItem[] EvaluateSTXE005(EvaluationContext context)
    {
        return context.IsApiController
            || context.TypeName.Split('.').Last() == "Program"
            ? Array.Empty<AnalysisItem>()
            : (
                from method in context
                    .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                    .OfType<MethodDeclarationSyntax>()
                    .Where(
                        delegate(MethodDeclarationSyntax method)
                        {
                            BlockSyntax? body = method.Body;
                            return body != null
                                && body.Statements.Count(
                                    (StatementSyntax statement) =>
                                        statement.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Any()
                                ) > 1;
                        }
                    )
                select CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXE005",
                    "An exposure must not sequence multiple routine calls.",
                    context,
                    method.GetLocation()
                )
            ).ToArray();
    }

    private static AnalysisItem[] EvaluateSTXAPI001(EvaluationContext context)
    {
        if (!context.IsApiController)
        {
            return Array.Empty<AnalysisItem>();
        }
        int serviceDependencyCount = context.Dependencies.Count(
            delegate(TypeDependency dependency)
            {
                StandardElementType standardElementType = dependency.StandardElementType;
                return (uint)(standardElementType - 1) <= 6u
                    || dependency.TypeName.EndsWith(
                        value: "Service",
                        comparisonType: StringComparison.Ordinal);
            }
        );
        return (serviceDependencyCount == 1)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXAPI001",
                    "An API controller must have exactly one business dependency.",
                    context
                ),
            };
    }

    private static AnalysisItem[] EvaluateSTXAPI002(EvaluationContext context)
    {
        return (!context.IsApiController || context.PublicApiModelTypes.Count <= 1)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXAPI002",
                    "An API controller must expose a single model contract.",
                    context
                ),
            };
    }

    private static AnalysisItem[] EvaluateSTXAPI003(EvaluationContext context)
    {
        string typeName = context.TypeName.Split('.').Last();
        return (!context.IsApiController || typeName.EndsWith("Controller", StringComparison.Ordinal))
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                    "STXAPI003",
                    "An API controller class name must end with Controller.",
                    context
                ),
            };
    }

    private static AnalysisItem[] EvaluateSTXAPI004(EvaluationContext context)
    {
        if (!context.IsApiController)
        {
            return Array.Empty<AnalysisItem>();
        }
        string[] verbs = new string[4] { "Get", "Post", "Put", "Delete" };
        return (
            from method in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                .OfType<MethodDeclarationSyntax>()
            where method.Modifiers.Any((SyntaxToken token) => token.RawKind == 8343)
            where !verbs.Any((string verb) => method.Identifier.Text.StartsWith(verb, StringComparison.Ordinal))
            select CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                "STXAPI004",
                "API controller methods must use HTTP nouns: Get, Post, Put, or Delete.",
                context,
                method.GetLocation()
            )
        ).ToArray();
    }
}
