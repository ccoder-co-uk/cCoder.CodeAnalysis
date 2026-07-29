// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Exposures;

internal static class StringExtensions
{
    internal static string Preserve(this string value) =>
        value.Length == 0
            ? string.Empty
            : value;
}