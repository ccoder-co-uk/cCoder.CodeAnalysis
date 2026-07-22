// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Models;

public sealed class TypeDependency
{
    public string TypeName { get; set; } = string.Empty;

    public StandardElementType StandardElementType { get; set; }
}