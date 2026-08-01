// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
#nullable disable
namespace cCoder.CodeAnalysis.Models;

internal sealed class ExceptionCatch
{
    public string ExceptionType { get; set; }
    public List<string> ThrownExceptionTypes { get; set; }
    public bool Rethrows { get; set; }
}
