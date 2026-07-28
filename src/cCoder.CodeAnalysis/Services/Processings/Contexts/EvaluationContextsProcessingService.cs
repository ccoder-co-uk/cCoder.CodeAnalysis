// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Contexts;

internal sealed class EvaluationContextsProcessingService : IEvaluationContextsProcessingService
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

    public IEnumerable<EvaluationContext> Process(ArchitectureBuild architectureBuild)
    {
        HashSet<string> localDependencyTypeNames = new HashSet<string>(
            collection: architectureBuild
                .DeclaredTypes.Where(
                    predicate: (INamedTypeSymbol type) => Classify(type: type) == StandardElementType.Dependency
                )
            .Select(selector: GetTypeName),
            comparer: StringComparer.Ordinal
        );

        return architectureBuild
            .DeclaredTypes.Where(predicate: (INamedTypeSymbol type) => type.TypeKind == TypeKind.Class)
            .Select(
                selector: (INamedTypeSymbol type) =>
                    CreateEvaluationContext(
                        type: type,
                        declaredTypes: architectureBuild.DeclaredTypes,
                        compilation: architectureBuild.Compilation,
                        projectLineEnding: architectureBuild.ProjectLineEnding,
                        localDependencyTypeNames: localDependencyTypeNames
                    )
            );
    }

    private static EvaluationContext CreateEvaluationContext(
        INamedTypeSymbol type,
        IReadOnlyCollection<INamedTypeSymbol> declaredTypes,
        CSharpCompilation compilation,
        string projectLineEnding,
        IReadOnlyCollection<string> localDependencyTypeNames
    )
    {
        TypeDeclarationSyntax? declaration = type
            .DeclaringSyntaxReferences.Select(selector: (SyntaxReference reference) => reference.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault();

        SyntaxTree? sourceTree =
            declaration?.SyntaxTree
            ?? (
                type.Name == "Program"
                    ? compilation.SyntaxTrees.FirstOrDefault(
                        predicate: (SyntaxTree tree) =>
                            tree.FilePath.EndsWith(value: "Program.cs", comparisonType: StringComparison.Ordinal)
                    )
                    : null
            );

        return new EvaluationContext
        {
            TypeName = GetTypeName(type: type),
            ProjectName = type.ContainingAssembly.Name,
            StandardElementType = Classify(type: type),
            LineNumber = declaration is null ? 0 : declaration.GetLocation()
            .GetLineSpan().StartLinePosition.Line + 1,
            IsPublic = type.DeclaredAccessibility == Accessibility.Public,
            IsConsoleApplication = compilation.Options.OutputKind == OutputKind.ConsoleApplication,
            IsApiController = IsApiController(type: type),
            HasBaseClass = type.BaseType != null && type.BaseType.SpecialType != SpecialType.System_Object,
            HasExternalBaseType = InheritsFromExternalType(type: type),
            ImplementsExternalInterface = ImplementsExternalInterface(type: type),
            ImplementsContract = type.AllInterfaces.Any(),
            HasExternalStateDependency = HasExternalStateDependency(type: type),
            DeclaresDependencyIntent = DeclaresDependencyIntent(type: type),
            Declarations = type
                .DeclaringSyntaxReferences.Select(selector: (SyntaxReference reference) => reference.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
                .ToArray(),
            FilePath = sourceTree?.FilePath ?? string.Empty,
            SourceCode = sourceTree?.GetText()
            .ToString() ?? string.Empty,
            ProjectLineEnding = projectLineEnding,
            UsingNamespaces =
                sourceTree
                    ?.GetRoot()
            .DescendantNodes()
                    .OfType<UsingDirectiveSyntax>()
                    .Select(selector: (UsingDirectiveSyntax item) => item.Name?.ToString() ?? string.Empty)
                    .Where(predicate: (string item) => item.Length != 0)
                    .ToArray()
                ?? [],
            Dependencies = type
                .InstanceConstructors.SelectMany(selector: (IMethodSymbol constructor) => constructor.Parameters)
            .Select(
                    selector: (IParameterSymbol parameter) =>
                        CreateTypeDependency(dependency: parameter.Type, declaredTypes: declaredTypes)
                )
                .GroupBy(
                    keySelector: (TypeDependency dependency) => dependency.TypeName,
                    comparer: StringComparer.Ordinal
                )
                .Select(selector: (IGrouping<string, TypeDependency> dependencies) => dependencies.First())
                .ToArray(),
            LocalDependencyTypeNames = localDependencyTypeNames,
            ImplementedInterfaces = type.AllInterfaces.Select(selector: GetTypeName)
            .ToArray(),
            PublicMethodNames = type.GetMembers()
            .OfType<IMethodSymbol>()
                .Where(
                    predicate: (IMethodSymbol method) =>
                        method.MethodKind == MethodKind.Ordinary && method.DeclaredAccessibility == Accessibility.Public
                )
                .Select(selector: (IMethodSymbol method) => method.Name)
                .Distinct(comparer: StringComparer.Ordinal)
                .ToArray(),
            ContractMethodNames = type
                .AllInterfaces.SelectMany(selector: (INamedTypeSymbol contract) => contract.GetMembers())
            .OfType<IMethodSymbol>()
                .Select(selector: (IMethodSymbol method) => method.Name)
                .Distinct(comparer: StringComparer.Ordinal)
                .ToArray(),
            PublicMethodCallLineNumbers = GetPublicMethodCallLineNumbers(type: type, compilation: compilation),
            PublicApiModelTypes = GetPublicApiModelTypes(type: type),
            ProjectTypeNames = declaredTypes.Select(selector: GetTypeName).ToArray(),
        };
    }

    private static string[] GetPublicApiModelTypes(INamedTypeSymbol type) =>

        type.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(
                predicate: (IMethodSymbol method) =>
                    method.MethodKind == MethodKind.Ordinary && method.DeclaredAccessibility == Accessibility.Public
            )
            .SelectMany(
                selector: (IMethodSymbol method) =>
                    method
                        .Parameters.Select(selector: (IParameterSymbol parameter) => parameter.Type)
            .Append(element: method.ReturnType)
            )
            .SelectMany(selector: GetContainedNamedTypes)
            .Where(
                predicate: (INamedTypeSymbol modelType) =>
                    modelType.TypeKind != TypeKind.Error
                    && modelType.ContainingAssembly is not null
                    && Classify(type: modelType) == StandardElementType.Model
            )
            .Select(selector: (INamedTypeSymbol modelType) => GetTypeName(type: modelType)
            .TrimEnd(trimChars: ['?']))
            .Distinct(comparer: StringComparer.Ordinal)
            .ToArray();

    private static IEnumerable<INamedTypeSymbol> GetContainedNamedTypes(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol arrayType)
        {
            return GetContainedNamedTypes(type: arrayType.ElementType);
        }

        if (type is not INamedTypeSymbol namedType)
        {
            return [];
        }

        return namedType.TypeArguments.Length == 0
            ? [namedType]
            : namedType.TypeArguments.SelectMany(selector: GetContainedNamedTypes);
    }

    private static int[] GetPublicMethodCallLineNumbers(INamedTypeSymbol type, CSharpCompilation compilation) =>

        type
            .DeclaringSyntaxReferences.Select(selector: (SyntaxReference reference) => reference.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.DescendantNodes())
            .OfType<InvocationExpressionSyntax>()
            .Where(
                predicate: (InvocationExpressionSyntax invocation) =>
                    IsPublicMethodCallOnSameType(invocation: invocation, type: type, compilation: compilation)
            )
            .Select(
                selector: (InvocationExpressionSyntax invocation) =>
                    invocation.GetLocation()
            .GetLineSpan().StartLinePosition.Line + 1
            )
            .ToArray();

    private static bool IsPublicMethodCallOnSameType(
        InvocationExpressionSyntax invocation,
        INamedTypeSymbol type,
        CSharpCompilation compilation
    )
    {
        SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree: invocation.SyntaxTree);
        MethodDeclarationSyntax? containingMethod = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();

        IMethodSymbol? caller = containingMethod is null
            ? null
            : semanticModel.GetDeclaredSymbol(declarationSyntax: containingMethod);

        return caller != null
            && caller.DeclaredAccessibility == Accessibility.Public
            && semanticModel.GetSymbolInfo(expression: invocation).Symbol is IMethodSymbol calledMethod
            && calledMethod.DeclaredAccessibility == Accessibility.Public
            && SymbolEqualityComparer.Default.Equals(x: calledMethod.ContainingType, y: type);
    }

    private static TypeDependency CreateTypeDependency(
        ITypeSymbol dependency,
        IReadOnlyCollection<INamedTypeSymbol> declaredTypes
    )
    {
        if (dependency is INamedTypeSymbol declaredType
            && Classify(type: declaredType) == StandardElementType.Exposure)
        {
            return new TypeDependency
            {
                TypeName = GetTypeName(type: declaredType),
                StandardElementType = StandardElementType.Exposure,
            };
        }

        INamedTypeSymbol? concreteType = ResolveConcreteType(dependency: dependency, declaredTypes: declaredTypes);

        return concreteType is null
            ? CreateReferencedTypeDependency(dependency: dependency)
            : new TypeDependency
            {
                TypeName = GetTypeName(type: concreteType),
                StandardElementType = Classify(type: concreteType),
            };
    }

    private static TypeDependency CreateReferencedTypeDependency(ITypeSymbol dependency)
    {
        StandardElementType elementType =
            dependency is INamedTypeSymbol namedType
            && namedType.ContainingAssembly is not null
            ? ClassifyReferencedType(type: namedType)
            : StandardElementType.Unknown;

        return new TypeDependency
        {
            TypeName = GetTypeName(type: dependency),
            StandardElementType =
                elementType == StandardElementType.Unknown ? StandardElementType.Dependency : elementType,
        };
    }

    private static StandardElementType ClassifyReferencedType(INamedTypeSymbol type) =>
        type.TypeKind == TypeKind.Interface
        && type.Name.EndsWith(value: "Service", comparisonType: StringComparison.Ordinal)
            ? StandardElementType.Exposure
            : Classify(type: type);

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

        if (containingNamespace.Contains(
            value: ".Activities.Activities",
            comparisonType: StringComparison.Ordinal))
        {
            return StandardElementType.Activity;
        }

        if (
            containingNamespace.Contains(value: ".Migrations", comparisonType: StringComparison.Ordinal)
            || InheritsFromExternalType(type: type)
            || (
                DeclaresDependencyIntent(type: type)
                && (
                    ImplementsExternalInterface(type: type)
                    || HasExternalStateDependency(type: type)
                )
            )
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

    private static bool IsApiController(INamedTypeSymbol type)
    {
        string containingNamespace = type.ContainingNamespace.ToDisplayString();

        if (containingNamespace.Contains(
            value: ".Controllers.Api",
            comparisonType: StringComparison.Ordinal))
        {
            return true;
        }

        for (INamedTypeSymbol? baseType = type.BaseType;
            baseType is not null;
            baseType = baseType.BaseType)
        {
            string baseTypeName = baseType.ToDisplayString();

            if (baseTypeName == "Microsoft.AspNetCore.Mvc.Controller")
            {
                return false;
            }

            if (baseTypeName == "Microsoft.AspNetCore.Mvc.ControllerBase")
            {
                return true;
            }
        }

        return type.GetAttributes()
            .Any(predicate: attribute =>
                attribute.AttributeClass?.Name == "ApiControllerAttribute")
            || containingNamespace.Contains(
                value: ".Controllers",
                comparisonType: StringComparison.Ordinal);
    }

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

        type.BaseType != null
        && type.BaseType.SpecialType != SpecialType.System_Object
        && !type.BaseType.Locations.Any(predicate: (Location location) => location.IsInSource);

    private static bool ImplementsExternalInterface(INamedTypeSymbol type) =>

        type.AllInterfaces.Any(
            predicate: (INamedTypeSymbol contract) =>
                !contract.Locations.Any(predicate: (Location location) => location.IsInSource)
        );

    private static bool HasExternalStateDependency(INamedTypeSymbol type) =>

        type.GetMembers()
            .OfType<IFieldSymbol>()
            .Any(
                predicate: (IFieldSymbol field) =>
                    field.Type.SpecialType == SpecialType.None
                    && !field.Type.Locations.Any(
                        predicate: (Location location) => location.IsInSource)
            );

    private static bool DeclaresDependencyIntent(INamedTypeSymbol type) =>

        type.ContainingNamespace.ToDisplayString()
            .Contains(value: ".Dependencies", comparisonType: StringComparison.Ordinal)
        || type.Name.EndsWith(value: "Dependency", comparisonType: StringComparison.Ordinal);

    private static bool IsStaticExtensionContainer(INamedTypeSymbol type) =>

        type.IsStatic
        && (
            type.ContainingNamespace.ToDisplayString()
                .Contains(value: ".Extensions", comparisonType: StringComparison.Ordinal)
            || type.Name.EndsWith(value: "Extensions", comparisonType: StringComparison.Ordinal)
        );

    private static string GetTypeName(ITypeSymbol type) =>
        type.ToDisplayString(format: FullyQualifiedTypeFormat);
}