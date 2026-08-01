// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
#nullable disable
using System.Text.Json.Serialization;
namespace cCoder.CodeAnalysis.Models;

public sealed class Architecture
{
    public int SchemaVersion { get; set; } = 2;
    public ProjectMetadata Project { get; set; } = new();
    public List<Class> Classes { get; set; } = [];
    public List<Link> Links { get; set; } = [];
    public List<AnalysisItem> AnalysisItems { get; set; } = [];

    [JsonIgnore]
    internal string AnalysisProjectLineEnding { get; set; } = string.Empty;

    [JsonIgnore]
    internal IReadOnlyCollection<string> AnalysisLocalDependencyTypeNames { get; set; } = [];
}
