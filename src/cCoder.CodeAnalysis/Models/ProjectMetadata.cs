// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
#nullable disable

namespace cCoder.CodeAnalysis.Models;

public sealed class ProjectMetadata
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AssemblyName { get; set; } = string.Empty;
}
