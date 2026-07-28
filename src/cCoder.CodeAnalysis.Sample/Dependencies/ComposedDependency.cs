// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Dependencies;

internal sealed class ComposedDependency(
    ExternalStateDependency externalStateDependency)
    : IDisposable
{
    public void Dispose() =>
        _ = externalStateDependency.CreateBytes(length: 1);
}