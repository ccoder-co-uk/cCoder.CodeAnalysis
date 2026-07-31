// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;

namespace cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;

internal interface IArchitectureModelQueriesProcessingService
{
    IReadOnlyList<TypeDependency> GetDependencies(EvaluationContext context);
    bool ImplementsContract(EvaluationContext context);
    bool HasMultipleTopLevelClasses(EvaluationContext context);
    IReadOnlyList<Method> GetReachableMethods(EvaluationContext context, string methodId);
    IReadOnlyCollection<string> GetEscapingExceptionTypes(EvaluationContext context, string methodId);
    bool CallsTypeMatching(EvaluationContext context, string methodId, string typeNameFragment);
}
