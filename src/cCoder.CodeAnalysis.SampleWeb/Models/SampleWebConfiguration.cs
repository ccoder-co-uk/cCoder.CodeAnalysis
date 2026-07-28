// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models;

namespace cCoder.CodeAnalysis.SampleWeb.Models;

public sealed class SampleWebConfiguration
{
    public SampleWebConfiguration() =>
        CodeAnalysisSample = new CodeAnalysisSampleConfiguration();

    public CodeAnalysisSampleConfiguration CodeAnalysisSample { get; set; }
}