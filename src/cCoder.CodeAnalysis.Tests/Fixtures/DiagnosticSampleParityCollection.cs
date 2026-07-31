// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Tests.Fixtures;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class DiagnosticSampleParityCollection :
    ICollectionFixture<DiagnosticSampleParityFixture>
{
    public const string Name = "Diagnostic sample parity";
}
