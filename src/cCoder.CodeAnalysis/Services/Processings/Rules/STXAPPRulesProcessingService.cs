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

        foreach (AnalysisItem item in EvaluateSTXAPP005(context: context))
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

        bool livesAtProjectRoot =
            fileName == $"{typeName}.cs"
            && parentFolder.Equals(value: context.ProjectName, comparisonType: StringComparison.Ordinal);

        return livesAtProjectRoot
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXAPP001",
                    description: "Program.cs, IServiceCollectionExtensions.cs, and WebApplicationExtensions.cs must live at the project root.",
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

        bool exposesDomainRegistration = GetMethods(context: context)
            .Any(
                predicate: (MethodDeclarationSyntax method) =>
                    method.Identifier.Text.StartsWith(value: "Add", comparisonType: StringComparison.Ordinal)
                    && !method.Identifier.Text.EndsWith(
                        value: "HostedServices",
                        comparisonType: StringComparison.Ordinal
                    )
                    && method.ParameterList.Parameters.Any(
                        predicate: (ParameterSyntax parameter) => parameter.Type?.ToString() == "IServiceCollection"
                    )
            );

        return exposesDomainRegistration
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXAPP002",
                    description: "IServiceCollectionExtensions must expose an Add{Domain} IServiceCollection extension.",
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

    private static IEnumerable<AnalysisItem> EvaluateSTXAPP005(EvaluationContext context)
    {
        if (context.TypeName.Split(separator: ['.'])
            .Last() != "Program")
        {
            return [];
        }

        int applicationNamespaceCount = context.UsingNamespaces.Count(
            predicate: (string item) =>
                !item.Equals(value: "System", comparisonType: StringComparison.Ordinal)
                && !item.StartsWith(value: "System.", comparisonType: StringComparison.Ordinal)
                && !item.Equals(value: "Microsoft", comparisonType: StringComparison.Ordinal)
                && !item.StartsWith(value: "Microsoft.", comparisonType: StringComparison.Ordinal)
        );

        bool usesServiceCollection =
            context.SourceCode.Contains(value: ".Services", comparisonType: StringComparison.Ordinal)
            && context.SourceCode.Contains(value: ".Add", comparisonType: StringComparison.Ordinal);

        return applicationNamespaceCount <= 1 && usesServiceCollection
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXAPP005",
                    description: "Program.cs must compose the app through IServiceCollection using SDK namespaces and one local composition namespace only.",
                    context: context
                ),
            ];
    }

    private static IEnumerable<MethodDeclarationSyntax> GetMethods(EvaluationContext context) =>

        context
            .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>();
}