// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Services.Processings;

namespace cCoder.CodeAnalysis.Sample.Dependencies;

internal sealed class ComposedProcessingDependency : IComposedProcessingService
{
    private readonly ExternalStateDependency externalStateDependency;

    internal ComposedProcessingDependency(ExternalStateDependency externalStateDependency)
    {
        this.externalStateDependency = externalStateDependency;
    }

    public void Execute() => _ = externalStateDependency.CreateBytes(length: 1);
}