// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Models;

public sealed class Method
{
    public string Name { get; set; } = string.Empty;

    public List<Input> Inputs { get; set; } = new List<Input>();

    public string ReturnType { get; set; } = string.Empty;
}