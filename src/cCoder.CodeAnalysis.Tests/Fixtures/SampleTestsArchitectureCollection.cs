// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Tests.Fixtures;

[CollectionDefinition("Sample tests architecture", DisableParallelization = true)]
public sealed class SampleTestsArchitectureCollection : ICollectionFixture<SampleTestsArchitectureFixture>
{
    public const string Name = "Sample tests architecture";
}