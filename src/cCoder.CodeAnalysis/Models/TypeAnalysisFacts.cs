// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
#nullable disable

namespace cCoder.CodeAnalysis.Models;

internal sealed class TypeAnalysisFacts
{
    internal string ProjectName { get; set; }
    internal string FilePath { get; set; }
    internal string SourceCode { get; set; }
    internal bool IsConsoleApplication { get; set; }
    internal IReadOnlyCollection<string> ProjectTypeNames { get; set; } = [];
    internal IReadOnlyList<MethodAnalysisFacts> Methods { get; set; } = [];
    internal IReadOnlyList<PropertyAnalysisFacts> Properties { get; set; } = [];
    internal bool AllDeclarationsArePartial { get; set; }
    internal int FirstNonPartialDeclarationLine { get; set; }
    internal int BaseTypeLine { get; set; }
}

internal sealed class MethodAnalysisFacts
{
    internal string Name { get; set; }
    internal int LineNumber { get; set; }
    internal bool IsPublic { get; set; }
    internal bool IsPrivate { get; set; }
    internal bool IsGeneric { get; set; }
    internal bool IsTest { get; set; }
    internal bool IsFact { get; set; }
    internal bool HasGivenWhenThenComments { get; set; }
    internal bool HasInvocations { get; set; }
    internal bool HasServiceCollectionParameter { get; set; }
    internal bool FirstParameterIsServiceCollectionExtension { get; set; }
    internal bool HasConfigurationParameter { get; set; }
    internal string ConfigurationCallbackType { get; set; }
    internal bool HasCommandDetailsParameter { get; set; }
    internal bool ResolvesServiceFromProvider { get; set; }
    internal bool PassesCommandDetails { get; set; }
    internal bool HasChainedServiceCollectionRegistration { get; set; }
    internal bool HasScopedOrTransientConfigurationRegistration { get; set; }
    internal IReadOnlyList<string> InvokedMethodNames { get; set; } = [];
}

internal sealed class PropertyAnalysisFacts
{
    internal string TypeName { get; set; }
    internal int LineNumber { get; set; }
    internal bool IsPublic { get; set; }
    internal bool HasGetter { get; set; }
    internal bool HasSetter { get; set; }
}
