// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
#nullable disable
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Models;

public sealed class Class
{
    public string Name { get; set; }
    public StandardElementType StandardElementType { get; set; }
    public int LineNumber { get; set; }
    public bool IsPublic { get; set; }
    public ArchitectureTypeKind Kind { get; set; } = ArchitectureTypeKind.Class;
    public TypeReference BaseType { get; set; }
    public List<TypeReference> Interfaces { get; set; } = [];
    public List<Property> Properties { get; set; } = [];
    public List<Method> Methods { get; set; } = [];

    [JsonIgnore]
    internal IReadOnlyList<TypeDependency> AnalysisDependencies { get; set; }

    [JsonIgnore]
    internal IReadOnlyList<string> AnalysisImplementedInterfaces { get; set; }

    [JsonIgnore]
    internal int AnalysisSourceFileTopLevelClassCount { get; set; }

    [JsonIgnore]
    internal bool AnalysisIsPrimaryTopLevelClassInFile { get; set; }

    [JsonIgnore]
    internal bool AnalysisIsApiController { get; set; }

    [JsonIgnore]
    internal IReadOnlyList<string> AnalysisPublicApiModelTypes { get; set; }

    [JsonIgnore]
    internal List<Method> AnalysisMethods { get; set; }

    [JsonIgnore]
    internal IReadOnlyList<TypeDeclarationSyntax> AnalysisDeclarations { get; set; }

    [JsonIgnore]
    internal string AnalysisFilePath { get; set; }

    [JsonIgnore]
    internal string AnalysisSourceCode { get; set; }

    [JsonIgnore]
    internal string AnalysisProjectLineEnding { get; set; }

    [JsonIgnore]
    internal TypeAnalysisFacts AnalysisTypeFacts { get; set; }

    [JsonIgnore]
    internal bool AnalysisHasExternalBaseType { get; set; }

    [JsonIgnore]
    internal bool AnalysisImplementsExternalInterface { get; set; }

    [JsonIgnore]
    internal bool AnalysisHasExternalStateDependency { get; set; }

    [JsonIgnore]
    internal bool AnalysisExposesExternalResource { get; set; }

    [JsonIgnore]
    internal bool AnalysisUsesExternalResource { get; set; }

    [JsonIgnore]
    internal bool AnalysisDeclaresDependencyIntent { get; set; }

    [JsonIgnore]
    internal IReadOnlyList<string> AnalysisContractMethodNames { get; set; }

    [JsonIgnore]
    internal IReadOnlyList<int> AnalysisPublicMethodCallLineNumbers { get; set; }
}
