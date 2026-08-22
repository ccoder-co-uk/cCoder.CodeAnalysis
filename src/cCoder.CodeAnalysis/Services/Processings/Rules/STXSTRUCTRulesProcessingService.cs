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
    private static readonly IArchitectureModelQueriesProcessingService architectureModelQueries =
        new ArchitectureModelQueriesProcessingService();

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateSTXSTRUCT001(context: context)
            .Concat(second: EvaluateSTXSTRUCT002(context: context))
            .Concat(second: EvaluateSTXSTRUCT003(context: context));
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXSTRUCT001(EvaluationContext context)
    {
        if (architectureModelQueries.GetTypeName(context).Split(separator: ['.'])
            .Last() == "Program")
        {
            return [];
        }

        return architectureModelQueries
            .GetDeclarations(context: context).Where(
                predicate: (Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax declaration) =>
                    !IsInStandardFolder(
                        filePath: declaration.SyntaxTree.FilePath,
                        elementType: architectureModelQueries.GetStandardElementType(context)
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
        ClassDeclarationSyntax? declaration = architectureModelQueries
            .GetDeclarations(context: context)
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
        if (!IsService(elementType: architectureModelQueries.GetStandardElementType(context)))
        {
            return [];
        }

        if (IsConsumedByLocalPublicExposure(context:context))
        {
            return [];
        }

        return architectureModelQueries.GetDeclarations(context: context)
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

    private static bool IsConsumedByLocalPublicExposure(EvaluationContext context)
    {
        string serviceTypeName = architectureModelQueries.GetTypeName(context:context);

        return context.ArchitectureModel.Classes.Any(predicate:element =>
            element.IsPublic
            && element.StandardElementType is StandardElementType.Exposure
                or StandardElementType.HttpExposure
            && ((element.AnalysisDependencies ?? []).Any(predicate:dependency =>
                    dependency.TypeName == serviceTypeName)
                || (element.Methods ?? []).SelectMany(selector:method => method.Calls ?? [])
                    .Any(predicate:call => call.TypeName == serviceTypeName)));
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
            StandardElementType.HttpExposure =>
                ["/Controllers/", "/Middleware/", "/Middlewares/", "/Exposures/"],
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
            Type = architectureModelQueries.GetTypeName(context),
            LineNumber = location.GetLineSpan().StartLinePosition.Line + 1,
        };
    }
}