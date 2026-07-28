// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Models;

internal sealed class DiagnosticCodeStandardPage
{
    public DiagnosticCodeStandardPage(string diagnosticCode, string? standardPageUri)
    {
        DiagnosticCode = diagnosticCode;
        StandardPageUris =
            standardPageUri is null
                ? Array.Empty<string>()
                : new string[] { standardPageUri };
    }

    public DiagnosticCodeStandardPage(
        string diagnosticCode,
        string firstStandardPageUri,
        string secondStandardPageUri)
    {
        DiagnosticCode = diagnosticCode;
        StandardPageUris = new string[] { firstStandardPageUri, secondStandardPageUri };
    }

    public DiagnosticCodeStandardPage(
        string diagnosticCode,
        string firstStandardPageUri,
        string secondStandardPageUri,
        string thirdStandardPageUri)
    {
        DiagnosticCode = diagnosticCode;

        StandardPageUris = new string[]
        {
            firstStandardPageUri,
            secondStandardPageUri,
            thirdStandardPageUri
        };
    }

    public string DiagnosticCode { get; }
    public IReadOnlyList<string> StandardPageUris { get; }
}
