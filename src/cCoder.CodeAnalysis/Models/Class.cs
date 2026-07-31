// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
#nullable disable
using System.Text.Json.Serialization;

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
    internal List<Method> AnalysisMethods { get; set; }
}
