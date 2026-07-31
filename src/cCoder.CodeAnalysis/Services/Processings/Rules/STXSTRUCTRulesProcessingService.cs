// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXSTRUCTRulesProcessingService : ISTXSTRUCTRulesProcessingService
{
    private readonly IArchitectureModelQueriesProcessingService architectureModelQueries =
        new ArchitectureModelQueriesProcessingService();

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateSTXSTRUCT001(context: context)
            .Concat(second: EvaluateSTXSTRUCT002(context: context))
            .Concat(second: EvaluateSTXSTRUCT003(context: context));
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXSTRUCT001(EvaluationContext context)
    {
        if (context.TypeName.Split(separator: ['.'])
            .Last() == "Program")
        {
            return [];
        }

        return context
            .Declarations.Where(
                predicate: (Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax declaration) =>
                    !IsInStandardFolder(
                        filePath: declaration.SyntaxTree.FilePath,
                        elementType: context.StandardElementType
                    )
            )
            .Select(
                selector: (Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax declaration) =>
                    CreateAnalysisItem(
                        code: "STXSTRUCT001",
                        description: "The source file must live in the standard folder for its element type.",
                        context: context,
                        location: declaration.GetLocation()
                    )
            );
    }

    private IEnumerable<AnalysisItem> EvaluateSTXSTRUCT002(EvaluationContext context)
    {
        ClassDeclarationSyntax? declaration = context.Declarations
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(predicate: IsTopLevelClass);

        return declaration is not null
            && architectureModelQueries.HasMultipleTopLevelClasses(context: context)
                ?
                [
                    CreateAnalysisItem(
                        code: "STXSTRUCT002",
                        description: "A source file must contain only one top-level class.",
                        context: context,
                        location: declaration.GetLocation())
                ]
                : [];
    }

    private static bool IsTopLevelClass(ClassDeclarationSyntax declaration) =>
        !declaration.Ancestors().OfType<TypeDeclarationSyntax>().Any();

    private static IEnumerable<AnalysisItem> EvaluateSTXSTRUCT003(
        EvaluationContext context)
    {
        if (!IsService(elementType: context.StandardElementType))
        {
            return [];
        }

        return context.Declarations
            .OfType<InterfaceDeclarationSyntax>()
            .Where(predicate: declaration =>
                declaration.Modifiers.Any(
                    kind: Microsoft.CodeAnalysis.CSharp.SyntaxKind
                        .PublicKeyword))
            .Select(selector: declaration =>
                CreateAnalysisItem(
                    code: "STXSTRUCT003",
                    description:
                        "Service contracts must be internal; expose cross-library operations through a public manager interface.",
                    context: context,
                    location: declaration.GetLocation()));
    }

    private static bool IsService(StandardElementType elementType) =>
        elementType is StandardElementType.FoundationService
            or StandardElementType.ProcessingService
            or StandardElementType.OrchestrationService
            or StandardElementType.CoordinationService
            or StandardElementType.ManagementService
            or StandardElementType.AggregationService;

    private static bool IsInStandardFolder(string filePath, StandardElementType elementType)
    {
        if (
            elementType is StandardElementType.Test or StandardElementType.App
            || string.IsNullOrWhiteSpace(value: filePath)
        )
        {
            return true;
        }

        string normalizedPath = filePath.Replace(oldChar: '\\', newChar: '/');

        string[] expectedFolders = elementType switch
        {
            StandardElementType.Broker => ["/Brokers/"],
            StandardElementType.Dependency => ["/Brokers/", "/Dependencies/", "/Exposures/"],
            StandardElementType.Exposure => ["/Exposures/", "/Controllers/", "/Extensions/"],
            StandardElementType.Model => ["/Models/"],
            StandardElementType.FoundationService => ["/Services/Foundations/"],
            StandardElementType.ProcessingService => ["/Services/Processings/"],
            StandardElementType.OrchestrationService => ["/Services/Orchestrations/"],
            StandardElementType.CoordinationService => ["/Services/Coordinations/"],
            StandardElementType.ManagementService => ["/Services/Managements/"],
            StandardElementType.AggregationService => ["/Services/Aggregations/"],
            _ => [],
        };

        return expectedFolders.Length == 0
            || expectedFolders.Any(
                predicate: (string folder) =>
                    normalizedPath.Contains(value: folder, comparisonType: StringComparison.OrdinalIgnoreCase)
            );
    }

    private static AnalysisItem CreateAnalysisItem(
        string code,
        string description,
        EvaluationContext context,
        Location location)
    {
        return new AnalysisItem
        {
            Code = code,
            Description = description,
            Severity = AnalysisSeverity.Warning,
            Type = context.TypeName,
            LineNumber = location.GetLineSpan().StartLinePosition.Line + 1,
        };
    }
}
