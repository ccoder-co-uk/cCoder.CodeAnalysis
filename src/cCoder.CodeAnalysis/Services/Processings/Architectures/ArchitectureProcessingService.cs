// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Foundations.Architectures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Architectures;

internal sealed class ArchitectureProcessingService(IArchitectureService architectureService)
    : IArchitectureProcessingService
{
    private static readonly SymbolDisplayFormat FullyQualifiedTypeFormat = new SymbolDisplayFormat(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        memberOptions: SymbolDisplayMemberOptions.None,
        delegateStyle: SymbolDisplayDelegateStyle.NameOnly,
        extensionMethodStyle: SymbolDisplayExtensionMethodStyle.Default,
        parameterOptions: SymbolDisplayParameterOptions.None,
        propertyStyle: SymbolDisplayPropertyStyle.NameOnly,
        localOptions: SymbolDisplayLocalOptions.None,
        kindOptions: SymbolDisplayKindOptions.None,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
    );

    public ArchitectureBuild Process(string path) =>

        Process(architectureBuild: architectureService.Build(projectFilePath: path));

    public ArchitectureBuild Process(CSharpCompilation compilation) =>

        Process(architectureBuild: architectureService.Build(compilation: compilation));

    private static ArchitectureBuild Process(ArchitectureBuild architectureBuild)
    {
        CSharpCompilation compilation = architectureBuild.Compilation;

        SyntaxTree[] projectSyntaxTrees = compilation
            .SyntaxTrees.Where(predicate: (SyntaxTree syntaxTree) =>
                syntaxTree.FilePath.Length > 0
                && !IsGeneratedSyntaxTree(syntaxTree))
            .ToArray();

        INamedTypeSymbol[] declaredTypes = projectSyntaxTrees
            .SelectMany(
                selector: (SyntaxTree syntaxTree) => GetDeclaredTypes(compilation: compilation, syntaxTree: syntaxTree)
            )
            .Where(
                predicate: (INamedTypeSymbol type) => type.Name is not "ValidationRule" and not "ValidationRulesEngine"
            )
            .GroupBy(keySelector: GetTypeName, comparer: StringComparer.Ordinal)
            .Select(selector: (IGrouping<string, INamedTypeSymbol> types) => types.First())
            .ToArray();

        Architecture architecture = new Architecture
        {
            Classes = declaredTypes
                .Where(predicate: (INamedTypeSymbol type) => type.TypeKind == TypeKind.Class)
            .Select(selector: CreateClass)
                .OrderBy(keySelector: (Class item) => item.Name, comparer: StringComparer.Ordinal)
                .ToList(),
            Links = declaredTypes
                .Where(predicate: (INamedTypeSymbol type) => type.TypeKind == TypeKind.Class)
            .SelectMany(selector: (INamedTypeSymbol type) => CreateLinks(type: type, declaredTypes: declaredTypes))
                .GroupBy(keySelector: (Link link) => (link.FromType, link.ToType))
                .Select(selector: (IGrouping<(string FromType, string ToType), Link> links) => links.First())
                .OrderBy(keySelector: (Link link) => link.FromType, comparer: StringComparer.Ordinal)
                .ThenBy(keySelector: (Link link) => link.ToType, comparer: StringComparer.Ordinal)
                .ToList(),
        };

        architectureBuild.Architecture = architecture;
        architectureBuild.DeclaredTypes = declaredTypes;

        architectureBuild.ProjectLineEnding =
            projectSyntaxTrees
                .Select(
                    selector: (SyntaxTree syntaxTree) => GetFirstLineEnding(source: syntaxTree.GetText()
            .ToString())
                )
                .FirstOrDefault(predicate: (string lineEnding) => lineEnding.Length > 0)
            ?? string.Empty;

        return architectureBuild;
    }

    private static bool IsGeneratedSyntaxTree(
        SyntaxTree syntaxTree)
    {
        string fileName = Path.GetFileName(syntaxTree.FilePath);

        if (fileName.EndsWith(
            ".g.cs",
            StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(
                ".g.i.cs",
                StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(
                ".AssemblyInfo.cs",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        string source = syntaxTree
            .GetText()
            .ToString();

        return source
            .TrimStart()
            .StartsWith(
                "// <auto-generated",
                StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<INamedTypeSymbol> GetDeclaredTypes(CSharpCompilation compilation, SyntaxTree syntaxTree)
    {
        SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree: syntaxTree);

        return syntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Select(
                selector: (TypeDeclarationSyntax declaration) =>
                    semanticModel.GetDeclaredSymbol(declarationSyntax: declaration)
            )
            .OfType<INamedTypeSymbol>();
    }

    private static Class CreateClass(INamedTypeSymbol type) =>

        new Class
        {
            Name = GetTypeName(type: type),
            StandardElementType = Classify(type: type),
            Properties = type.GetMembers()
            .OfType<IPropertySymbol>()
                .Where(predicate: (IPropertySymbol property) => property.DeclaredAccessibility == Accessibility.Public)
                .Select(
                    selector: (IPropertySymbol property) =>
                        new Property { Name = property.Name, Type = GetTypeName(type: property.Type) }
                )
                .OrderBy(keySelector: (Property property) => property.Name, comparer: StringComparer.Ordinal)
                .ToList(),
            Methods = type.GetMembers()
            .OfType<IMethodSymbol>()
                .Where(
                    predicate: (IMethodSymbol method) =>
                        method.MethodKind == MethodKind.Ordinary && method.DeclaredAccessibility == Accessibility.Public
                )
                .Select(
                    selector: (IMethodSymbol method) =>
                        new Method
                        {
                            Name = method.Name,
                            ReturnType = GetTypeName(type: method.ReturnType),
                            Inputs = method
                                .Parameters.Select(
                                    selector: (IParameterSymbol parameter) =>
                                        new Input { Name = parameter.Name, Type = GetTypeName(type: parameter.Type) }
                                )
            .ToList(),
                        }
                )
                .OrderBy(keySelector: (Method method) => method.Name, comparer: StringComparer.Ordinal)
                .ToList(),
        };

    private static IEnumerable<Link> CreateLinks(
        INamedTypeSymbol type,
        IReadOnlyCollection<INamedTypeSymbol> declaredTypes
    )
    {
        IEnumerable<ITypeSymbol> dependencies = type
            .InstanceConstructors.SelectMany(selector: (IMethodSymbol constructor) => constructor.Parameters)
            .Select(selector: (IParameterSymbol parameter) => parameter.Type);

        foreach (ITypeSymbol dependency in dependencies)
        {
            INamedTypeSymbol? target = ResolveConcreteType(dependency: dependency, declaredTypes: declaredTypes);

            if (target is not null)
            {
                yield return new Link { FromType = GetTypeName(type: type), ToType = GetTypeName(type: target) };
            }
        }
    }

    private static INamedTypeSymbol? ResolveConcreteType(
        ITypeSymbol dependency,
        IReadOnlyCollection<INamedTypeSymbol> declaredTypes
    )
    {
        if (
            dependency.TypeKind == TypeKind.Class
            && declaredTypes.Contains(value: dependency, comparer: SymbolEqualityComparer.Default)
        )
        {
            return (INamedTypeSymbol)dependency;
        }

        if (!dependency.Locations.Any(predicate: (Location location) => location.IsInSource))
        {
            return null;
        }

        INamedTypeSymbol[] implementations = declaredTypes
            .Where(
                predicate: (INamedTypeSymbol type) =>
                    type.TypeKind == TypeKind.Class
                    && type.AllInterfaces.Contains(value: dependency, comparer: SymbolEqualityComparer.Default)
            )
            .Take(count: 2)
            .ToArray();

        return implementations.Length == 1 ? implementations[0] : null;
    }

    private static StandardElementType Classify(INamedTypeSymbol type)
    {
        string containingNamespace = type.ContainingNamespace.ToDisplayString();

        if (
            type.ContainingAssembly.Name.EndsWith(value: "Tests", comparisonType: StringComparison.Ordinal)
            || containingNamespace.Contains(value: ".Tests", comparisonType: StringComparison.Ordinal)
        )
        {
            return StandardElementType.Test;
        }

        if (
            type.Name
                is "Program"
                or "IServiceCollectionExtensions"
                or "IHostExtensions"
                or "WebApplicationExtensions"
            || type.Name.EndsWith(
                value: "BuilderOptions",
                comparisonType: StringComparison.Ordinal)
            || IsConfigurationCompositionHelper(type: type)
        )
        {
            return StandardElementType.App;
        }

        if (
            containingNamespace.Contains(value: ".Controllers", comparisonType: StringComparison.Ordinal)
            || containingNamespace.Contains(value: ".Exposures", comparisonType: StringComparison.Ordinal)
            || type.Name.EndsWith(value: "EventHub", comparisonType: StringComparison.Ordinal)
            || type.Name is "EventProvider" or "BulkEventProvider"
            || IsStaticExtensionContainer(type: type)
        )
        {
            return StandardElementType.Exposure;
        }

        if (
            containingNamespace.Contains(value: ".Migrations", comparisonType: StringComparison.Ordinal)
            || InheritsFromExternalType(type: type)
        )
        {
            return StandardElementType.Dependency;
        }

        if (containingNamespace.Contains(value: ".Services.Foundations", comparisonType: StringComparison.Ordinal))
        {
            return StandardElementType.FoundationService;
        }

        if (containingNamespace.Contains(value: ".Services.Processings", comparisonType: StringComparison.Ordinal))
        {
            return StandardElementType.ProcessingService;
        }

        if (containingNamespace.Contains(value: ".Services.Orchestrations", comparisonType: StringComparison.Ordinal))
        {
            return StandardElementType.OrchestrationService;
        }

        if (containingNamespace.Contains(value: ".Services.Coordinations", comparisonType: StringComparison.Ordinal))
        {
            return StandardElementType.CoordinationService;
        }

        if (containingNamespace.Contains(value: ".Services.Managements", comparisonType: StringComparison.Ordinal))
        {
            return StandardElementType.ManagementService;
        }

        if (containingNamespace.Contains(value: ".Services.Aggregations", comparisonType: StringComparison.Ordinal))
        {
            return StandardElementType.AggregationService;
        }

        if (containingNamespace.Contains(value: ".Models", comparisonType: StringComparison.Ordinal))
        {
            return StandardElementType.Model;
        }

        if (containingNamespace.Contains(value: ".Brokers", comparisonType: StringComparison.Ordinal))
        {
            return StandardElementType.Broker;
        }

        if (ImplementsExternalInterface(type: type))
        {
            return StandardElementType.Dependency;
        }

        if (IsDataOnlyType(type: type))
        {
            return StandardElementType.Model;
        }

        return StandardElementType.Unknown;
    }

    private static bool IsConfigurationCompositionHelper(
        INamedTypeSymbol type) =>
        type.IsStatic
        && type.ContainingNamespace.ToDisplayString()
            == type.ContainingAssembly.Name
        && (
            type.Name.EndsWith(
                value: "ConfigurationMapper",
                comparisonType: StringComparison.Ordinal)
            || type.Name.EndsWith(
                value: "UrlResolver",
                comparisonType: StringComparison.Ordinal)
        );

    private static bool IsDataOnlyType(INamedTypeSymbol type) =>

        type.TypeKind == TypeKind.Class
        && type
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Any()
        && !type
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Any(predicate: (IMethodSymbol method) => method.MethodKind == MethodKind.Ordinary && !method.IsOverride);

    private static bool InheritsFromExternalType(INamedTypeSymbol type) =>

        type.BaseType is not null
        && type.BaseType.SpecialType != SpecialType.System_Object
        && !type.BaseType.Locations.Any(predicate: (Location location) => location.IsInSource);

    private static bool ImplementsExternalInterface(INamedTypeSymbol type) =>

        type.Interfaces.Any(
            predicate: (INamedTypeSymbol contract) =>
                !contract.Locations.Any(predicate: (Location location) => location.IsInSource)
        );

    private static bool IsStaticExtensionContainer(INamedTypeSymbol type) =>

        type.IsStatic
        && (
            type.ContainingNamespace.ToDisplayString()
                .Contains(value: ".Extensions", comparisonType: StringComparison.Ordinal)
            || type.Name.EndsWith(value: "Extensions", comparisonType: StringComparison.Ordinal)
        );

    private static string GetTypeName(ITypeSymbol type) =>
        type.ToDisplayString(format: FullyQualifiedTypeFormat);

    private static string GetFirstLineEnding(string source)
    {
        for (int index = 0; index < source.Length; index++)
        {
            if (source[index: index] == '\r')
            {
                return index < source.Length - 1 && source[index: index + 1] == '\n' ? "\r\n" : "\r";
            }

            if (source[index: index] == '\n')
            {
                return "\n";
            }
        }

        return string.Empty;
    }
}
