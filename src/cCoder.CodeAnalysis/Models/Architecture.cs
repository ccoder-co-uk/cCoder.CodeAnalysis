// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
#nullable disable
namespace cCoder.CodeAnalysis.Models;

public sealed class Architecture
{
    public List<Class> Classes { get; set; }
    public List<Link> Links { get; set; }
    public List<AnalysisItem> AnalysisItems { get; set; }
}