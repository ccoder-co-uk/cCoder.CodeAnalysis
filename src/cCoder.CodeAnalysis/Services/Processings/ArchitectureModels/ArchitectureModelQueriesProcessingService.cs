// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;

internal sealed class ArchitectureModelQueriesProcessingService
    : IArchitectureModelQueriesProcessingService
{
    public string GetTypeName(EvaluationContext context) =>
        context.ArchitectureElement.Name;

    public StandardElementType GetStandardElementType(EvaluationContext context) =>
        context.ArchitectureElement.StandardElementType;

    public int GetLineNumber(EvaluationContext context) =>
        context.ArchitectureElement.LineNumber;

    public bool IsPublic(EvaluationContext context) =>
        context.ArchitectureElement.IsPublic;

    public TypeReference? GetBaseType(EvaluationContext context) =>
        context.ArchitectureElement.BaseType;

    public bool HasExternalBaseType(EvaluationContext context) =>
        context.ArchitectureElement.AnalysisHasExternalBaseType;

    public bool ImplementsExternalInterface(EvaluationContext context) =>
        context.ArchitectureElement.AnalysisImplementsExternalInterface;

    public bool HasExternalStateDependency(EvaluationContext context) =>
        context.ArchitectureElement.AnalysisHasExternalStateDependency;

    public bool ExposesExternalResource(EvaluationContext context) =>
        context.ArchitectureElement.AnalysisExposesExternalResource;

    public bool UsesExternalResource(EvaluationContext context) =>
        context.ArchitectureElement.AnalysisUsesExternalResource;

    public bool DeclaresDependencyIntent(EvaluationContext context) =>
        context.ArchitectureElement.AnalysisDeclaresDependencyIntent;

    public IReadOnlyList<TypeDependency> GetDependencies(EvaluationContext context) =>
        context.ArchitectureElement.AnalysisDependencies ?? [];

    public IReadOnlyCollection<string> GetLocalDependencyTypeNames(EvaluationContext context) =>
        context.ArchitectureModel.AnalysisLocalDependencyTypeNames;

    public bool ImplementsContract(EvaluationContext context) =>
        context.ArchitectureElement.AnalysisImplementedInterfaces.Count != 0;

    public bool HasMultipleTopLevelClasses(EvaluationContext context) =>
        context.ArchitectureElement.AnalysisIsPrimaryTopLevelClassInFile
        && context.ArchitectureElement.AnalysisSourceFileTopLevelClassCount > 1;

    public bool IsApiController(EvaluationContext context) =>
        context.ArchitectureElement.AnalysisIsApiController;

    public IReadOnlyList<string> GetPublicApiModelTypes(EvaluationContext context) =>
        context.ArchitectureElement.AnalysisPublicApiModelTypes ?? [];

    public IReadOnlyList<string> GetImplementedInterfaces(EvaluationContext context) =>
        context.ArchitectureElement.AnalysisImplementedInterfaces ?? [];

    public IReadOnlyList<string> GetPublicMethodNames(EvaluationContext context) =>
        context.ArchitectureElement.Methods
            .Select(method => method.Name)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    public IReadOnlyList<string> GetContractMethodNames(EvaluationContext context) =>
        context.ArchitectureElement.AnalysisContractMethodNames ?? [];

    public IReadOnlyList<int> GetPublicMethodCallLineNumbers(EvaluationContext context) =>
        context.ArchitectureElement.AnalysisPublicMethodCallLineNumbers ?? [];

    public IReadOnlyCollection<string> GetProjectTypeNames(EvaluationContext context) =>
        context.ArchitectureModel.Classes.Select(element => element.Name).ToArray();

    public string GetProjectName(EvaluationContext context) =>
        context.ArchitectureModel.Project.AssemblyName;

    public IReadOnlyList<TypeDeclarationSyntax> GetDeclarations(EvaluationContext context) =>
        context.ArchitectureElement.AnalysisDeclarations ?? [];

    public string GetFilePath(EvaluationContext context) =>
        context.ArchitectureElement.AnalysisFilePath ?? string.Empty;

    public string GetSourceCode(EvaluationContext context) =>
        context.ArchitectureElement.AnalysisSourceCode ?? string.Empty;

    public string GetProjectLineEnding(EvaluationContext context) =>
        context.ArchitectureModel.AnalysisProjectLineEnding;

    public IReadOnlyList<Method> GetReachableMethods(EvaluationContext context, string methodId)
    {
        Dictionary<string, Method> methodsById = GetMethodsById(context: context);
        Dictionary<string, Method[]> implementationsByContractId = methodsById.Values
            .SelectMany(selector: method => (method.Implements ?? []).Select(
                selector: contractId => (contractId, method)))
            .GroupBy(keySelector: item => item.contractId, comparer: StringComparer.Ordinal)
            .ToDictionary(
                keySelector: group => group.Key,
                elementSelector: group => group.Select(selector: item => item.method).ToArray(),
                comparer: StringComparer.Ordinal);

        if (!methodsById.TryGetValue(key: methodId, value: out Method? root))
        {
            return [];
        }

        List<Method> reachableMethods = [];
        HashSet<string> visitedMethodIds = new(StringComparer.Ordinal);
        Stack<Method> pendingMethods = new();
        pendingMethods.Push(item: root);

        while (pendingMethods.Count != 0)
        {
            Method method = pendingMethods.Pop();

            if (!visitedMethodIds.Add(item: method.Id))
            {
                continue;
            }

            reachableMethods.Add(item: method);

            foreach (MethodCall call in method.DirectCalls ?? [])
            {
                if (!call.IsDependencyBoundary
                    && methodsById.TryGetValue(key: call.MethodId, value: out Method? calledMethod))
                {
                    pendingMethods.Push(item: calledMethod);
                }

                if (!call.IsDependencyBoundary
                    && implementationsByContractId.TryGetValue(
                        key: call.MethodId,
                        value: out Method[]? implementations))
                {
                    foreach (Method implementation in implementations)
                    {
                        pendingMethods.Push(item: implementation);
                    }
                }
            }
        }

        return reachableMethods;
    }

    public IReadOnlyCollection<string> GetEscapingExceptionTypes(
        EvaluationContext context,
        string methodId) =>
        GetMethodsById(context: context).TryGetValue(key: methodId, value: out Method? method)
            ? method.ThrowsExceptionTypes
            : [];

    public bool CallsTypeMatching(
        EvaluationContext context,
        string methodId,
        string typeNameFragment) =>
        GetReachableMethods(context: context, methodId: methodId)
            .SelectMany(selector: method => method.DirectCalls ?? [])
            .Any(predicate: call => call.TypeName.Contains(
                value: typeNameFragment,
                comparisonType: StringComparison.Ordinal));

    private static Dictionary<string, Method> GetMethodsById(EvaluationContext context) =>
        (context.ArchitectureModel?.Classes ?? [])
            .SelectMany(selector: element => element.AnalysisMethods ?? [])
            .ToDictionary(keySelector: method => method.Id, comparer: StringComparer.Ordinal);
}
