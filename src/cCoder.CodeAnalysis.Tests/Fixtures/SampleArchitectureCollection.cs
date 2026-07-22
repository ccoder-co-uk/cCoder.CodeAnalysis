// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Tests.Fixtures;

[CollectionDefinition("Sample architecture", DisableParallelization = true)]
public sealed class SampleArchitectureCollection : ICollectionFixture<SampleArchitectureFixture>
{
    public const string Name = "Sample architecture";
}
