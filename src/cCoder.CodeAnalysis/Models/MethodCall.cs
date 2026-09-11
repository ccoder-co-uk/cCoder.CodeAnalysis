// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
#nullable disable
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;

namespace cCoder.CodeAnalysis.Models;

public sealed class MethodCall
{
    public string TypeName { get; set; }
    public string MethodName { get; set; }
    public string MethodId { get; set; }
    public StandardElementType StandardElementType { get; set; }
    public bool IsDependencyBoundary { get; set; }

    [JsonIgnore]
    internal IMethodSymbol TargetSymbol { get; set; }

    [JsonIgnore]
    internal bool IsExceptionWrapper { get; set; }

    [JsonIgnore]
    internal bool IsExternalApiCall { get; set; }

    [JsonIgnore]
    internal bool IsInsideLambda { get; set; }

    [JsonIgnore]
    internal bool IsTargetCallbackParameter { get; set; }

    [JsonIgnore]
    internal string ArchitecturalDependencyTypeName { get; set; }

    [JsonIgnore]
    internal StandardElementType? ArchitecturalDependencyStandardElementType { get; set; }

    [JsonIgnore]
    internal IReadOnlyList<ITypeSymbol> ServiceLocatorTypeArguments { get; set; }

    [JsonIgnore]
    internal int SourceLineNumber { get; set; }
}