// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Tests.BuildTasks;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class BuildTaskGlobalStateCollection
{
    public const string Name = "Build task global state";
}
