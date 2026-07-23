// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class ModelCodeAnalysisRulesProcessingService
    : CodeAnalysisRulesProcessingService,
        IModelCodeAnalysisRulesProcessingService
{
    public AnalysisItem[] Evaluate(EvaluationContext context)
    {
        return CodeAnalysisRulesProcessingService
            .EvaluateSourceFormatting(context)
            .Concat(ModelCodeAnalysisRulesProcessingService.EvaluateSTXM001(context))
            .Concat(ModelCodeAnalysisRulesProcessingService.EvaluateSTXM002(context))
            .Concat(ModelCodeAnalysisRulesProcessingService.EvaluateSTXM003(context))
            .ToArray();
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXM001(EvaluationContext context)
    {
        return (
            from method in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                .OfType<MethodDeclarationSyntax>()
            where !method.Modifiers.Any(
                modifier => modifier.RawKind == (int)SyntaxKind.OverrideKeyword)
            select CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                "STXM001",
                "Models must not declare methods.",
                context,
                method.GetLocation()
            )
        );
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXM002(EvaluationContext context)
    {
        return (
            from property in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                .OfType<PropertyDeclarationSyntax>()
            where property.Initializer is not null
            select CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                "STXM002",
                "Model properties must not declare default values.",
                context,
                property.Initializer!.GetLocation()
            )
        );
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXM003(EvaluationContext context)
    {
        string[] validationAttributeNames =
        [
            "Compare",
            "CreditCard",
            "CustomValidation",
            "DataType",
            "EmailAddress",
            "MaxLength",
            "MinLength",
            "Phone",
            "Range",
            "RegularExpression",
            "Required",
            "StringLength",
            "Url",
            "Validation"
        ];

        return (
            from property in context
                .Declarations.SelectMany((TypeDeclarationSyntax declaration) => declaration.Members)
                .OfType<PropertyDeclarationSyntax>()
            where property.Modifiers.Any(
                    (SyntaxToken modifier) => modifier.IsKind(SyntaxKind.RequiredKeyword))
                || property.AttributeLists
                    .SelectMany((AttributeListSyntax attributeList) => attributeList.Attributes)
                    .Any(attribute =>
                        validationAttributeNames.Contains(
                            attribute.Name.ToString().Split('.').Last().Replace("Attribute", "")))
            select CodeAnalysisRulesProcessingService.CreateAnalysisItem(
                "STXM003",
                "Model properties must not use framework validation attributes or the required modifier; validation belongs in service collectors.",
                context,
                property.GetLocation()
            )
        );
    }
}
