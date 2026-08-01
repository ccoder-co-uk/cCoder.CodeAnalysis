// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Reflection;
using cCoder.CodeAnalysis.Exposures;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.StandardElementTypes;

public sealed class RuleProcessingServiceConventionTests
{
    [Fact]
    public void RuleProcessingServicesShouldExposeOneNamedEvaluationMethodPerRegisteredRule()
    {
        Type ruleProcessingServiceType = typeof(IRuleProcessingService);
        Type[] implementationTypes = ruleProcessingServiceType.Assembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(ruleProcessingServiceType.IsAssignableFrom)
            .ToArray();
        string[] registeredCodes = DiagnosticCodeStandardPageIndex
            .GetDiagnosticCodeStandardPages()
            .Select(page => page.DiagnosticCode)
            .ToArray();

        foreach (Type implementationType in implementationTypes)
        {
            const string suffix = "RulesProcessingService";
            string prefix = implementationType.Name[..^suffix.Length];
            string[] ownedCodes = registeredCodes
                .Where(code => code.StartsWith(prefix, StringComparison.Ordinal))
                .Where(code => code[prefix.Length..].All(char.IsDigit))
                .ToArray();
            string[] methodNames = implementationType
                .GetMethods(
                    BindingFlags.Instance
                        | BindingFlags.Static
                        | BindingFlags.Public
                        | BindingFlags.NonPublic)
                .Select(method => method.Name)
                .ToArray();

            methodNames.Should().Contain(
                ownedCodes.Select(code => $"Evaluate{code}"),
                $"{implementationType.Name} must expose one explicitly named evaluation method per owned rule");
        }
    }
}
