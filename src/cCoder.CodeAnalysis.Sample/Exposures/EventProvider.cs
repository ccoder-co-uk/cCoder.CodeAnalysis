// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Models.Compatibility;

internal sealed class EventProvider
{
    internal string? Name { get; set; }

    internal void Invoke()
    {
        if (string.IsNullOrWhiteSpace(value: Name))
        {
            Name = nameof(EventProvider);
        }
    }
}