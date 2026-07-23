// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Dependencies;

internal sealed class CompatibilityDependency
{
    private bool lastValue;

    public bool Resolve(bool value)
    {
        lastValue = value;

        if (value)
        {
            return true;
        }

        return lastValue;
    }

    internal void Invoke()
    {
    }
}
