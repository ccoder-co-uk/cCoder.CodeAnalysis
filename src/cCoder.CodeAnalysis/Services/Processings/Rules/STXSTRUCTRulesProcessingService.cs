// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXSTRUCTRulesProcessingService : ISTXSTRUCTRulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateSTXSTRUCT001(context: context);
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
                    CreateAnalysisItem(context: context, location: declaration.GetLocation())
            );
    }

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

    private static AnalysisItem CreateAnalysisItem(EvaluationContext context, Location location)
    {
        return new AnalysisItem
        {
            Code = "STXSTRUCT001",
            Description = "The source file must live in the standard folder for its element type.",
            Severity = AnalysisSeverity.Warning,
            Type = context.TypeName,
            LineNumber = location.GetLineSpan().StartLinePosition.Line + 1,
        };
    }
}
