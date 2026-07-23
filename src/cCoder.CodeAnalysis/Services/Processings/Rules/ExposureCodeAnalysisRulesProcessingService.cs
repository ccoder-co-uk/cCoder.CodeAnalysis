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

    private static AnalysisItem[] EvaluateSTXE001(EvaluationContext context)
    {
        return context.IsApiController
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

                return (uint)(standardElementType - 1) <= 6u
                    || dependency.TypeName.EndsWith(
                        value: "Service",
                        comparisonType: StringComparison.Ordinal);
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
                return (uint)(standardElementType - 1) <= 6u;
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