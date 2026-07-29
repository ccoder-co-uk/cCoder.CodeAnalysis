// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Dependencies;

internal sealed class ExternalResourceLeakingDependency : IDisposable
{
    internal System.IO.StreamReader Reader { get; init; } = null!;

    public void Dispose() =>
        Reader.Dispose();
}