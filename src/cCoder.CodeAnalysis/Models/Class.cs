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
    public List<Property> Properties { get; set; }
    public List<Method> Methods { get; set; }

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
}
