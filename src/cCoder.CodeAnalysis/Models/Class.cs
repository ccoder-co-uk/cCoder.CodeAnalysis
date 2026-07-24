// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
#nullable disable
namespace cCoder.CodeAnalysis.Models;

public sealed class Class
{
    public string Name { get; set; }
    public StandardElementType StandardElementType { get; set; }
    public List<Property> Properties { get; set; }
    public List<Method> Methods { get; set; }
}