// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
#nullable disable
namespace cCoder.CodeAnalysis.Models;

public sealed class AnalysisItem
{
    public string Code { get; set; }
    public string Description { get; set; }
    public AnalysisSeverity Severity { get; set; }
    public string Type { get; set; }
    public int LineNumber { get; set; }
}