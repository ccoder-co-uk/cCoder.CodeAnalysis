// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Tests.Fixtures;

[CollectionDefinition("SampleAcceptanceTestsArchitectureCollection", DisableParallelization = true)]
public sealed class SampleAcceptanceTestsArchitectureCollection
    : ICollectionFixture<SampleAcceptanceTestsArchitectureFixture>
{
    public const string Name = "SampleAcceptanceTestsArchitectureCollection";
}