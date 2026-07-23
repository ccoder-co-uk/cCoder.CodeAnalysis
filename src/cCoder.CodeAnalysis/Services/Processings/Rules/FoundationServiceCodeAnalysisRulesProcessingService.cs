// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class FoundationServiceCodeAnalysisRulesProcessingService
    : CodeAnalysisRulesProcessingService,
        IFoundationServiceCodeAnalysisRulesProcessingService
{
    public AnalysisItem[] Evaluate(EvaluationContext context)
    {
        List<AnalysisItem> list = new List<AnalysisItem>();
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateSourceFormatting(context));
        list.AddRange(FoundationServiceCodeAnalysisRulesProcessingService.EvaluateSTXF001(context));
        list.AddRange(FoundationServiceCodeAnalysisRulesProcessingService.EvaluateSTXF002(context));
        list.AddRange(FoundationServiceCodeAnalysisRulesProcessingService.EvaluateSTXF003(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluatePropertiesAreNotAllowed(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateRedundantPassThroughService(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateFlowForward(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluatePublicApiFlowForward(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateBusinessImplementationVisibility(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateSingleServiceContract(context));
        list.AddRange(CodeAnalysisRulesProcessingService.EvaluateServiceContractPattern(context));
        return list.ToArray();
    }

    private static AnalysisItem[] EvaluateSTXF001(EvaluationContext context)
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
                    && !node.SyntaxTree.FilePath.EndsWith(".Validations.cs", StringComparison.Ordinal)
                    && context.PublicApiModelTypes.Any(
                        (string modelType) =>
                            node
                                .DescendantNodes()
                                .OfType<IdentifierNameSyntax>()
                                .Any(
                                    (IdentifierNameSyntax identifier) =>
                                        identifier.Identifier.Text
                                        == modelType.Substring(modelType.LastIndexOf('.') + 1)
                                )
                    )
            select new AnalysisItem
            {
                Code = "STXF001",
                Description = "A foundation service must not loop over its service model type.",
                Severity = AnalysisSeverity.Warning,
                Type = context.TypeName,
                LineNumber = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
            }
        ).ToArray();
    }

    private static AnalysisItem[] EvaluateSTXF002(EvaluationContext context)
    {
        return (
            !context.Dependencies.Any(
                delegate(TypeDependency dependency)
                {
                    StandardElementType standardElementType = dependency.StandardElementType;
                    return standardElementType != StandardElementType.Broker
                        && standardElementType != StandardElementType.Exposure;
                }
            )
        )
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                new AnalysisItem
                {
                    Code = "STXF002",
                    Description = "A foundation service may only depend on brokers, exposures, or nothing.",
                    Severity = AnalysisSeverity.Warning,
                    Type = context.TypeName,
                    LineNumber = context.LineNumber,
                },
            };
    }

    private static AnalysisItem[] EvaluateSTXF003(EvaluationContext context)
    {
        return context
            .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .SelectMany(
                (MethodDeclarationSyntax method) =>
                    method
                        .ParameterList.Parameters.Where(
                            (ParameterSyntax parameter) =>
                                context.PublicApiModelTypes.Any(
                                    (string modelType) =>
                                        modelType.EndsWith($".{parameter.Type}", StringComparison.Ordinal)
                                )
                        )
                        .SelectMany(
                            (ParameterSyntax parameter) =>
                                from invocation in method.DescendantNodes().OfType<InvocationExpressionSyntax>()
                                where
                                    invocation.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax
                                    && (
                                        memberAccessExpressionSyntax.Name.Identifier.Text.StartsWith(
                                            "Insert",
                                            StringComparison.Ordinal
                                        )
                                        || memberAccessExpressionSyntax.Name.Identifier.Text.StartsWith(
                                            "Update",
                                            StringComparison.Ordinal
                                        )
                                    )
                                where
                                    invocation.ArgumentList.Arguments.Any(
                                        (ArgumentSyntax argument) =>
                                            argument.Expression is IdentifierNameSyntax { Identifier: var identifier }
                                            && identifier.Text == parameter.Identifier.Text
                                    )
                                select CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                                    "STXF003",
                                    "Foundation mutations must pass a flat single-row model to their broker.",
                                    context,
                                    invocation.GetLocation()
                                )
                        )
            )
            .ToArray();
    }
}
