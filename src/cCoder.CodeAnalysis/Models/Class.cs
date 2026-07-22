// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Models;

public sealed class Class
{
    public string Name { get; set; } = string.Empty;

    public StandardElementType StandardElementType { get; set; }

    public List<Property> Properties { get; set; } = new List<Property>();

    public List<Method> Methods { get; set; } = new List<Method>();
}