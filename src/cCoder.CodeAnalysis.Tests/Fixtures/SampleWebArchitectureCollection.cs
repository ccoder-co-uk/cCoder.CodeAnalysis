// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Tests.Fixtures;

[CollectionDefinition("SampleWebArchitectureCollection", DisableParallelization = true)]
public sealed class SampleWebArchitectureCollection : ICollectionFixture<SampleWebArchitectureFixture>
{
    public const string Name = "SampleWebArchitectureCollection";
}
