// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.Architectures;

internal sealed class ArchitectureGraphProcessingService : IArchitectureGraphProcessingService
{
    public ArchitectureBuild Process(ArchitectureBuild architectureBuild)
    {
        Method[] methods = architectureBuild.Architecture.Classes
            .SelectMany(element => element.AnalysisMethods)
            .ToArray();

        Dictionary<string, Method> methodsById = methods
            .GroupBy(method => method.Id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        Dictionary<string, Method[]> implementationsByContractId = methods
            .SelectMany(method => method.Implements.Select(contractId => (contractId, method)))
            .GroupBy(item => item.contractId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(item => item.method).Distinct().ToArray(),
                StringComparer.Ordinal);

        Dictionary<string, IReadOnlyCollection<string>> exceptionCache = new(StringComparer.Ordinal);
        Dictionary<string, IReadOnlyCollection<string>> possibleExceptionCache = new(StringComparer.Ordinal);

        foreach (Method method in methods)
        {
            method.PossibleExceptionTypes = GetPossibleExceptionTypes(
                    method: method,
                    methodsById: methodsById,
                    implementationsByContractId: implementationsByContractId,
                    exceptionCache: possibleExceptionCache,
                    activeMethodIds: new HashSet<string>(StringComparer.Ordinal))
                .OrderBy(typeName => typeName, StringComparer.Ordinal)
                .ToList();

            method.ThrowsExceptionTypes = GetEscapingExceptionTypes(
                    method: method,
                    methodsById: methodsById,
                    implementationsByContractId: implementationsByContractId,
                    exceptionCache: exceptionCache,
                    activeMethodIds: new HashSet<string>(StringComparer.Ordinal))
                .OrderBy(typeName => typeName, StringComparer.Ordinal)
                .ToList();
        }

        return architectureBuild;
    }

    private static IReadOnlyCollection<string> GetPossibleExceptionTypes(
        Method method,
        IReadOnlyDictionary<string, Method> methodsById,
        IReadOnlyDictionary<string, Method[]> implementationsByContractId,
        IDictionary<string, IReadOnlyCollection<string>> exceptionCache,
        ISet<string> activeMethodIds)
    {
        if (exceptionCache.TryGetValue(method.Id, out IReadOnlyCollection<string>? cached))
        {
            return cached;
        }

        if (!activeMethodIds.Add(method.Id))
        {
            return [];
        }

        HashSet<string> possibleExceptions = new(
            method.DirectlyThrowsExceptionTypes,
            StringComparer.Ordinal);

        foreach (MethodCall call in method.DirectCalls.Where(call => !call.IsDependencyBoundary))
        {
            foreach (Method calledMethod in ResolveCalledMethods(
                call: call,
                methodsById: methodsById,
                implementationsByContractId: implementationsByContractId))
            {
                possibleExceptions.UnionWith(
                    GetPossibleExceptionTypes(
                        method: calledMethod,
                        methodsById: methodsById,
                        implementationsByContractId: implementationsByContractId,
                        exceptionCache: exceptionCache,
                        activeMethodIds: activeMethodIds));
            }
        }

        activeMethodIds.Remove(method.Id);

        string[] result = possibleExceptions
            .OrderBy(typeName => typeName, StringComparer.Ordinal)
            .ToArray();

        exceptionCache[method.Id] = result;
        return result;
    }

    private static IReadOnlyCollection<string> GetEscapingExceptionTypes(
        Method method,
        IReadOnlyDictionary<string, Method> methodsById,
        IReadOnlyDictionary<string, Method[]> implementationsByContractId,
        IDictionary<string, IReadOnlyCollection<string>> exceptionCache,
        ISet<string> activeMethodIds)
    {
        if (exceptionCache.TryGetValue(method.Id, out IReadOnlyCollection<string>? cached))
        {
            return cached;
        }

        if (!activeMethodIds.Add(method.Id))
        {
            return [];
        }

        HashSet<string> propagatedExceptions = new(StringComparer.Ordinal);

        foreach (MethodCall call in method.DirectCalls.Where(call => !call.IsDependencyBoundary))
        {
            foreach (Method calledMethod in ResolveCalledMethods(
                call: call,
                methodsById: methodsById,
                implementationsByContractId: implementationsByContractId))
            {
                propagatedExceptions.UnionWith(
                    GetEscapingExceptionTypes(
                        method: calledMethod,
                        methodsById: methodsById,
                        implementationsByContractId: implementationsByContractId,
                        exceptionCache: exceptionCache,
                        activeMethodIds: activeMethodIds));
            }
        }

        foreach (ExceptionCatch exceptionCatch in method.ExceptionCatches)
        {
            propagatedExceptions.RemoveWhere(
                exceptionType => CatchHandles(
                    exceptionType: exceptionType,
                    caughtExceptionType: exceptionCatch.ExceptionType));

            if (exceptionCatch.Rethrows)
            {
                propagatedExceptions.Add(exceptionCatch.ExceptionType);
            }
        }

        propagatedExceptions.UnionWith(method.DirectlyThrowsExceptionTypes);
        activeMethodIds.Remove(method.Id);

        string[] result = propagatedExceptions
            .OrderBy(typeName => typeName, StringComparer.Ordinal)
            .ToArray();

        exceptionCache[method.Id] = result;
        return result;
    }

    private static IEnumerable<Method> ResolveCalledMethods(
        MethodCall call,
        IReadOnlyDictionary<string, Method> methodsById,
        IReadOnlyDictionary<string, Method[]> implementationsByContractId)
    {
        if (methodsById.TryGetValue(call.MethodId, out Method? method))
        {
            yield return method;
        }

        if (implementationsByContractId.TryGetValue(call.MethodId, out Method[]? implementations))
        {
            foreach (Method implementation in implementations)
            {
                yield return implementation;
            }
        }
    }

    private static bool CatchHandles(string exceptionType, string caughtExceptionType) =>

        caughtExceptionType == "System.Exception"
        || string.Equals(exceptionType, caughtExceptionType, StringComparison.Ordinal);
}
