// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Models;

public sealed class CodeAnalysisSampleConfiguration
{
    public CodeAnalysisSampleConfiguration() =>
        ConnectionString = string.Empty;

    public string ConnectionString { get; set; }
}