// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;

internal interface IArchitectureModelQueriesProcessingService
{
    string GetTypeName(EvaluationContext context);
    StandardElementType GetStandardElementType(EvaluationContext context);
    int GetLineNumber(EvaluationContext context);
    bool IsPublic(EvaluationContext context);
    TypeReference? GetBaseType(EvaluationContext context);
    bool HasExternalBaseType(EvaluationContext context);
    bool ImplementsExternalInterface(EvaluationContext context);
    bool HasExternalStateDependency(EvaluationContext context);
    bool ExposesExternalResource(EvaluationContext context);
    bool UsesExternalResource(EvaluationContext context);
    bool DeclaresDependencyIntent(EvaluationContext context);
    IReadOnlyList<TypeDependency> GetDependencies(EvaluationContext context);
    IReadOnlyCollection<string> GetLocalDependencyTypeNames(EvaluationContext context);
    bool ImplementsContract(EvaluationContext context);
    bool HasMultipleTopLevelClasses(EvaluationContext context);
    bool IsApiController(EvaluationContext context);
    IReadOnlyList<string> GetPublicApiModelTypes(EvaluationContext context);
    IReadOnlyList<string> GetImplementedInterfaces(EvaluationContext context);
    IReadOnlyList<string> GetPublicMethodNames(EvaluationContext context);
    IReadOnlyList<string> GetContractMethodNames(EvaluationContext context);
    IReadOnlyList<int> GetPublicMethodCallLineNumbers(EvaluationContext context);
    IReadOnlyCollection<string> GetProjectTypeNames(EvaluationContext context);
    string GetProjectName(EvaluationContext context);
    IReadOnlyList<TypeDeclarationSyntax> GetDeclarations(EvaluationContext context);
    string GetFilePath(EvaluationContext context);
    string GetSourceCode(EvaluationContext context);
    string GetProjectLineEnding(EvaluationContext context);
    IReadOnlyList<Method> GetReachableMethods(EvaluationContext context, string methodId);
    IReadOnlyCollection<string> GetEscapingExceptionTypes(EvaluationContext context, string methodId);
    bool CallsTypeMatching(EvaluationContext context, string methodId, string typeNameFragment);
}
