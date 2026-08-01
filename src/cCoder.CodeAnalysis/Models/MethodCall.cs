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
}