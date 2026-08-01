// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
#nullable disable

namespace cCoder.CodeAnalysis.Models;

public sealed class TypeReference
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string AssemblyName { get; set; } = string.Empty;
    public ArchitectureTypeKind Kind { get; set; }
    public bool IsInCurrentProject { get; set; }
    public StandardElementType StandardElementType { get; set; }
}
