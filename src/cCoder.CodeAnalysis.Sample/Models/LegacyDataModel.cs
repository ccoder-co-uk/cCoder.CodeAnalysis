// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample;

#pragma warning disable CS8618
internal sealed class LegacyDataModel
{
    public string Value { get; set; }

    public override string ToString() =>
        Value;
}
#pragma warning restore CS8618