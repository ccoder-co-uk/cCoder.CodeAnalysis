// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
#nullable disable
namespace cCoder.CodeAnalysis.Models;

public sealed class TypeDependency
{
    public string TypeName { get; set; }
    public StandardElementType StandardElementType { get; set; }
}