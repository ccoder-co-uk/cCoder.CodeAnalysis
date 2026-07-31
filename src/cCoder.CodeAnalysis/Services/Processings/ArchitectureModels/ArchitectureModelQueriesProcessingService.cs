// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;

internal sealed class ArchitectureModelQueriesProcessingService
    : IArchitectureModelQueriesProcessingService
{
    public IReadOnlyList<TypeDependency> GetDependencies(EvaluationContext context) =>
        context.ArchitectureElement?.AnalysisDependencies ?? context.Dependencies;

    public bool ImplementsContract(EvaluationContext context) =>
        (context.ArchitectureElement?.AnalysisImplementedInterfaces ?? context.ImplementedInterfaces).Count != 0;

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
