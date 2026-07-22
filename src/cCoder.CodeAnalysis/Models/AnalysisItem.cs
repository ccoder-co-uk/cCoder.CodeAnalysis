// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Models;

public sealed class AnalysisItem
{
    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public AnalysisSeverity Severity { get; set; } = AnalysisSeverity.Warning;

    public string Type { get; set; } = string.Empty;

    public int LineNumber { get; set; }
}