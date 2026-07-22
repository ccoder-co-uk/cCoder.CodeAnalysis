// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Models;

public sealed class Architecture
{
    public List<Class> Classes { get; set; } = new List<Class>();

    public List<Link> Links { get; set; } = new List<Link>();

    public List<AnalysisItem> AnalysisItems { get; set; } = new List<AnalysisItem>();
}