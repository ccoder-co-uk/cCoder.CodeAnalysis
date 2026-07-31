// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
#nullable disable
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;

namespace cCoder.CodeAnalysis.Models;

public sealed class Method
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int LineNumber { get; set; }
    public List<Input> Inputs { get; set; }
    public string ReturnType { get; set; }
    public List<string> Implements { get; set; }
    public List<MethodCall> Calls { get; set; }
    public List<string> PossibleExceptionTypes { get; set; }
    public List<string> ThrowsExceptionTypes { get; set; }
    public List<string> HttpMethods { get; set; }
    public List<HttpResponse> HttpResponses { get; set; }
    public bool IsHttpRequestHandler { get; set; }
    public bool IsODataControllerAction { get; set; }
    public bool HasFromBodyParameter { get; set; }
    public bool HasKeyParameter { get; set; }
    public bool HandlesNullWithNotFound { get; set; }

    [JsonIgnore]
    internal IMethodSymbol Symbol { get; set; }

    [JsonIgnore]
    internal List<MethodCall> DirectCalls { get; set; }

    [JsonIgnore]
    internal List<string> DirectlyThrowsExceptionTypes { get; set; }

    [JsonIgnore]
    internal List<ExceptionCatch> ExceptionCatches { get; set; }
}
