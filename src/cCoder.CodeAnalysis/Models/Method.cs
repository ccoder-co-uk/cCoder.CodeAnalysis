// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
#nullable disable
namespace cCoder.CodeAnalysis.Models;

public sealed class Method
{
    public string Name { get; set; }
    public List<Input> Inputs { get; set; }
    public string ReturnType { get; set; }
}