// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
#nullable disable
namespace cCoder.CodeAnalysis.Models;

public sealed class HttpResponse
{
    public int StatusCode { get; set; }
    public string ResultMethod { get; set; }
    public string ExceptionType { get; set; }
    public bool IsExceptionPath { get; set; }
    public bool IsNullPath { get; set; }
    public bool ExposesExceptionDetails { get; set; }
}
