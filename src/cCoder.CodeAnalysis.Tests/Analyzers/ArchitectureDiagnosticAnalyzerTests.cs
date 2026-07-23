// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Analyzers;
using FluentAssertions;
using Microsoft.CodeAnalysis;

namespace cCoder.CodeAnalysis.Tests.Analyzers;

public sealed class ArchitectureDiagnosticAnalyzerTests
{
    [Fact]
    public void ShouldLinkEveryDiagnosticToItsRuleDocumentation()
    {
        ArchitectureDiagnosticAnalyzer analyzer = new ArchitectureDiagnosticAnalyzer();

        foreach (DiagnosticDescriptor descriptor in analyzer.SupportedDiagnostics)
        {
            string prefix = new string(
                descriptor.Id.TakeWhile(character => !char.IsDigit(character)).ToArray()
            );

            descriptor.HelpLinkUri.Should().Be(
                $"https://ccoder.co.uk/Documentation/CodeAnalysis/{prefix}/{descriptor.Id}"
            );
        }
    }
}
