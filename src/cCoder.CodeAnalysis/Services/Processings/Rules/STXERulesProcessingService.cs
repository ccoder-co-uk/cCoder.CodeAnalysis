// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXERulesProcessingService : ISTXERulesProcessingService
{
    private static readonly IArchitectureModelQueriesProcessingService architectureModelQueries =
        new ArchitectureModelQueriesProcessingService();

    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        TypeAnalysisFacts? facts = context.ArchitectureElement?.AnalysisTypeFacts;

        string typeName = architectureModelQueries.GetTypeName(context)
            .Split(separator: ['.'])
            .Last();

        if (typeName.EndsWith(
            "Extensions",
            StringComparison.Ordinal))
        {
            IEnumerable<AnalysisItem> extensionContainerRules = facts is null
                ? []
                : EvaluateSTXE007(
                    context: context,
                    facts: facts,
                    extensionContainerName: typeName);

            return EvaluateSTXE006(
                    context: context,
                    facts: facts)
                .Concat(second: extensionContainerRules);
        }

        return EvaluateSTXE001(context: context, facts: facts)
            .Concat(second: EvaluateSTXE002(context: context, facts: facts))
            .Concat(second: EvaluateSTXE003(context: context))
            .Concat(second: EvaluateSTXE004(context: context))
            .Concat(second: EvaluateSTXE005(context: context, facts: facts))
            .Concat(second: EvaluateSTXE008(context: context));
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXE006(
        EvaluationContext context,
        TypeAnalysisFacts? facts) =>
        facts is null
        || !facts.Methods.Any(method => method.IsExtensionMethod)
            ?
            [
                CreateAnalysisItem(
                    code: "STXE006",
                    description:
                        "A type named Extensions must declare at least one extension method.",
                    context: context)
            ]
            : [];

    private static AnalysisItem CreateAnalysisItem(
        string code,
        string description,
        EvaluationContext context,
        int lineNumber = 0
    )
    {
        return new AnalysisItem
        {
            Code = code,
            Description = description,
            Severity = AnalysisSeverity.Warning,
            Type = architectureModelQueries.GetTypeName(context),
            LineNumber = lineNumber > 0
                ? lineNumber
                : architectureModelQueries.GetLineNumber(context),
        };
    }

    private IEnumerable<AnalysisItem> EvaluateSTXE001(
        EvaluationContext context,
        TypeAnalysisFacts? facts)
    {
        return architectureModelQueries.IsApiController(context: context)
            || architectureModelQueries.GetTypeName(context).Split(separator: ['.']).Last() == "Program"
            || IsEventProviderContract(context: context)
            ? []
            : (facts?.BranchingLineNumbers ?? [])
                .Except(facts?.MvcActionResponseBranchingLineNumbers ?? [])
                .Select(
                    selector: lineNumber =>
                        CreateAnalysisItem(
                            code: "STXE001",
                            description: "An exposure must not contain branching logic.",
                            context: context,
                            lineNumber: lineNumber
                        )
                );
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXE002(
        EvaluationContext context,
        TypeAnalysisFacts? facts) =>
        IsEventProviderContract(context: context)
            ? []
            : (facts?.LoopLineNumbers ?? [])
            .Select(
                selector: lineNumber =>
                    CreateAnalysisItem(
                        code: "STXE002",
                        description: "An exposure must not contain loops.",
                        context: context,
                        lineNumber: lineNumber
                    )
            );

    private static bool IsEventProviderContract(EvaluationContext context)
    {
        string typeName = architectureModelQueries.GetTypeName(context).Split(separator: ['.']).Last();

        return typeName is "EventProvider" or "BulkEventProvider"
            || typeName.StartsWith(value: "EventProvider<", comparisonType: StringComparison.Ordinal)
            || typeName.StartsWith(value: "BulkEventProvider<", comparisonType: StringComparison.Ordinal);
    }

    private static IEnumerable<AnalysisItem> EvaluateSTXE007(
        EvaluationContext context,
        TypeAnalysisFacts facts,
        string extensionContainerName) =>
        facts.Methods
            .Where(method => method.IsExtensionMethod)
            .Where(
                predicate: method =>
                    !MatchesExtensionContainer(
                        receiverName: method.ExtensionReceiverTypeName,
                        extensionContainerName: extensionContainerName))
            .Select(
                selector: method =>
                    CreateAnalysisItem(
                        code: "STXE007",
                        description:
                            "An extension method must be declared in the Extensions type named for its receiver.",
                        context: context,
                        lineNumber: method.LineNumber));

    private static bool MatchesExtensionContainer(
        string receiverName,
        string extensionContainerName)
    {
        int genericStart = receiverName.IndexOf(
            value: '<');

        if (genericStart >= 0)
        {
            receiverName = receiverName.Substring(
                startIndex: 0,
                length: genericStart);
        }

        receiverName = receiverName
            .Split(separator: new[] { '.' })
            .Last()
            .TrimEnd(trimChars: new[] { '?' });

        receiverName = receiverName switch
        {
            "bool" => "Boolean",
            "byte" => "Byte",
            "char" => "Char",
            "decimal" => "Decimal",
            "double" => "Double",
            "float" => "Single",
            "int" => "Int32",
            "long" => "Int64",
            "object" => "Object",
            "sbyte" => "SByte",
            "short" => "Int16",
            "string" => "String",
            "uint" => "UInt32",
            "ulong" => "UInt64",
            "ushort" => "UInt16",
            _ => receiverName,
        };

        string exactContainerName =
            $"{receiverName}Extensions";

        string interfaceContainerName =
            receiverName.Length > 1
            && receiverName[0] == 'I'
            && char.IsUpper(c: receiverName[1])
                ? $"{receiverName.Substring(startIndex: 1)}Extensions"
                : exactContainerName;

        return extensionContainerName == exactContainerName
            || extensionContainerName == interfaceContainerName;
    }

    private IEnumerable<AnalysisItem> EvaluateSTXE003(EvaluationContext context)
    {
        if (architectureModelQueries.IsApiController(context: context)
            || IsStandardizedProviderClient(context))
        {
            return [];
        }

        int serviceDependencyCount = architectureModelQueries.GetDependencies(context: context).Count(
            predicate: (TypeDependency dependency) =>
                IsBusinessService(dependency.StandardElementType)
        );

        return serviceDependencyCount <= 1
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXE003",
                    description: "An exposure may communicate with only one business service.",
                    context: context
                ),
            ];
    }

    private static bool IsBusinessService(StandardElementType elementType) =>
        elementType is StandardElementType.FoundationService
            or StandardElementType.ProcessingService
            or StandardElementType.OrchestrationService
            or StandardElementType.CoordinationService
            or StandardElementType.ManagementService
            or StandardElementType.AggregationService;

    private bool IsStandardizedProviderClient(
        EvaluationContext context) =>
        architectureModelQueries.GetProjectName(context).EndsWith(
            value: ".Providers",
            comparisonType: StringComparison.OrdinalIgnoreCase) == true
        && architectureModelQueries.GetTypeName(context).Split(separator: ['.'])
            .Last()
            .EndsWith(
                value: "Client",
                comparisonType: StringComparison.Ordinal)
        && architectureModelQueries.GetImplementedInterfaces(context: context).Any(
            predicate: interfaceName =>
                interfaceName.Split(separator: ['.'])
                    .Last()
                    .EndsWith(
                        value: "Client",
                        comparisonType: StringComparison.Ordinal));

    private IEnumerable<AnalysisItem> EvaluateSTXE004(EvaluationContext context)
    {
        return !architectureModelQueries.GetDependencies(context: context).Any(
            predicate: (TypeDependency dependency) => dependency.StandardElementType == StandardElementType.Broker
        )
            ? []
            :
            [
                CreateAnalysisItem(
                    code: "STXE004",
                    description: "An exposure must not communicate directly with a broker.",
                    context: context
                ),
            ];
    }

    private IEnumerable<AnalysisItem> EvaluateSTXE005(
        EvaluationContext context,
        TypeAnalysisFacts? facts)
    {
        return architectureModelQueries.IsApiController(context: context)
            || IsHostedService(context: context)
            || architectureModelQueries.GetTypeName(context).Split(separator: ['.']).Last() == "Program"
            ? []
            : (facts?.Methods ?? [])
                .Where(method => method.HasMultipleRoutineCallStatements)
                .Select(
                    selector: method =>
                        CreateAnalysisItem(
                            code: "STXE005",
                            description: "An exposure must not sequence multiple routine calls.",
                            context: context,
                            lineNumber: method.LineNumber
                        )
                );
    }

    private bool IsHostedService(
        EvaluationContext context) =>
        architectureModelQueries.GetImplementedInterfaces(context: context).Any(
            predicate: (string interfaceName) =>
                interfaceName.EndsWith(
                    value: ".IHostedService",
                    comparisonType: StringComparison.Ordinal)
                || interfaceName == "IHostedService");

    private static IEnumerable<AnalysisItem> EvaluateSTXE008(EvaluationContext context) =>
        (context.ArchitectureElement?.Methods ?? [])
            .SelectMany(method => method.ExceptionCatches ?? [])
            .Where(exceptionCatch =>
                (exceptionCatch.Rethrows || exceptionCatch.ThrownExceptionTypes.Count > 0)
                && !exceptionCatch.LogsException)
            .Select(_ => CreateAnalysisItem(
                code: "STXE008",
                description: "An exposure must log a caught exception before rethrowing or wrapping it.",
                context: context));

}
