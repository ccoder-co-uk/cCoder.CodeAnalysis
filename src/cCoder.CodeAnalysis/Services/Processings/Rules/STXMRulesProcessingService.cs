// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXMRulesProcessingService : ISTXMRulesProcessingService
{
    private static readonly IArchitectureModelQueriesProcessingService architectureModelQueries =
        new ArchitectureModelQueriesProcessingService();

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateSTXM001(context: context)
            .Concat(second: EvaluateSTXM002(context: context))
            .Concat(second: EvaluateSTXM003(context: context));
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
            Type = architectureModelQueries.GetTypeName(context: context),
            LineNumber = location is null
                ? architectureModelQueries.GetLineNumber(context: context)
                : location.GetLineSpan().StartLinePosition.Line + 1,
        };
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXM001(EvaluationContext context)
    {
        bool isConfigurationModel = architectureModelQueries.GetTypeName(context: context)
            .Split(separator: ['.'])
            .Last()
            .EndsWith(value: "Configuration", comparisonType: StringComparison.Ordinal);

        return architectureModelQueries
            .GetDeclarations(context: context)
            .SelectMany(
                selector: (TypeDeclarationSyntax declaration) =>
                    declaration
                        .Members.OfType<BaseMethodDeclarationSyntax>()
                        .Cast<SyntaxNode>()
                        .Concat(
                            second: declaration.ParameterList is null
                                ? []
                                : [declaration.ParameterList]
                        )
                        .Where(member =>
                            !isConfigurationModel
                            || member is not ConstructorDeclarationSyntax)
            )
            .Select(
                selector: (SyntaxNode method) =>
                    CreateAnalysisItem(
                        code: "STXM001",
                        description: "Models must not declare methods, constructors, destructors, or operators.",
                        context: context,
                        location: method.GetLocation()
                    )
            );
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXM002(EvaluationContext context) =>

        architectureModelQueries
            .GetDeclarations(context: context)
            .SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<PropertyDeclarationSyntax>()
            .Where(predicate: (PropertyDeclarationSyntax property) => property.Initializer is not null)
            .Select(
                selector: (PropertyDeclarationSyntax property) =>
                    CreateAnalysisItem(
                        code: "STXM002",
                        description: "Model properties must not declare default values.",
                        context: context,
                        location: property.Initializer!.GetLocation()
                    )
            );

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
            "Validation",
        ];

        return architectureModelQueries
            .GetDeclarations(context: context)
            .SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<PropertyDeclarationSyntax>()
            .Where(
                predicate: (PropertyDeclarationSyntax property) =>
                    property.Modifiers.Any(
                        predicate: (SyntaxToken modifier) => modifier.IsKind(kind: SyntaxKind.RequiredKeyword)
                    )
                    || property
                        .AttributeLists.SelectMany(
                            selector: (AttributeListSyntax attributeList) => attributeList.Attributes
                        )
            .Any(predicate: attribute =>
                            validationAttributeNames.Contains(
                                value: attribute
                                    .Name.ToString()
            .Split(separator: '.')
                                    .Last()
                                    .Replace(oldValue: "Attribute", newValue: "")
                            )
                        )
            )
            .Select(
                selector: (PropertyDeclarationSyntax property) =>
                    CreateAnalysisItem(
                        code: "STXM003",
                        description: "Model properties must not use framework validation attributes or the required modifier; validation belongs in service collectors.",
                        context: context,
                        location: property.GetLocation()
                    )
            );
    }
}
