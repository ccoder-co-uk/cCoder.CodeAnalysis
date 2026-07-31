// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
#nullable disable
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Models;

public sealed class EvaluationContext
{
    [JsonIgnore]
    internal Architecture ArchitectureModel { get; set; }

    [JsonIgnore]
    internal Class ArchitectureElement { get; set; }

    public string TypeName { get; set; }
    public StandardElementType StandardElementType { get; set; }
    public int LineNumber { get; set; }
    public bool IsPublic { get; set; }
    public bool IsApiController { get; set; }
    public bool IsConsoleApplication { get; set; }
    public bool HasBaseClass { get; set; }
    public bool HasExternalBaseType { get; set; }
    public bool ImplementsExternalInterface { get; set; }
    public bool ImplementsContract { get; set; }
    public bool HasExternalStateDependency { get; set; }
    public bool ExposesExternalResource { get; set; }
    public bool UsesExternalResource { get; set; }
    public bool DeclaresDependencyIntent { get; set; }
    public IReadOnlyList<TypeDeclarationSyntax> Declarations { get; set; }
    public IReadOnlyList<TypeDependency> Dependencies { get; set; }
    public IReadOnlyCollection<string> LocalDependencyTypeNames { get; set; }
    public IReadOnlyList<string> ImplementedInterfaces { get; set; }
    public IReadOnlyList<string> PublicMethodNames { get; set; }
    public IReadOnlyList<string> ContractMethodNames { get; set; }
    public IReadOnlyList<int> PublicMethodCallLineNumbers { get; set; }
    public IReadOnlyList<string> PublicApiModelTypes { get; set; }
    public IReadOnlyCollection<string> ProjectTypeNames { get; set; }
    public string FilePath { get; set; }
    public IReadOnlyList<string> UsingNamespaces { get; set; }
    public string ProjectName { get; set; }
    public string SourceCode { get; set; }
    public string ProjectLineEnding { get; set; }
}
