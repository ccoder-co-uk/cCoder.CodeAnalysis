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

        Architecture architecture = architectureBuild.Architecture
            ?? CreateArchitectureShell(
                declaredTypes: architectureBuild.DeclaredTypes,
                compilation: architectureBuild.Compilation);

        architecture.AnalysisProjectLineEnding = architectureBuild.ProjectLineEnding;
        architecture.AnalysisLocalDependencyTypeNames = localDependencyTypeNames;

        return architectureBuild
            .DeclaredTypes.Where(
                predicate: (INamedTypeSymbol type) =>
                    type.TypeKind is TypeKind.Class or TypeKind.Interface)
            .Select(
                selector: (INamedTypeSymbol type) =>
                    CreateEvaluationContext(
                        type: type,
                        declaredTypes: architectureBuild.DeclaredTypes,
                        compilation: architectureBuild.Compilation,
                        architecture: architecture)
            );
    }

    private static Architecture CreateArchitectureShell(
        IReadOnlyCollection<INamedTypeSymbol> declaredTypes,
        CSharpCompilation compilation) =>
        new()
        {
            Project = new ProjectMetadata
            {
                Id = compilation.AssemblyName ?? string.Empty,
                Name = compilation.AssemblyName ?? string.Empty,
                AssemblyName = compilation.AssemblyName ?? string.Empty,
            },
            Classes = declaredTypes
                .Where(type => type.TypeKind == TypeKind.Class)
                .Select(CreateArchitectureElement)
                .ToList(),
            Interfaces = declaredTypes
                .Where(type => type.TypeKind == TypeKind.Interface)
                .Select(CreateArchitectureElement)
                .ToList(),
        };

    private static Class CreateArchitectureElement(INamedTypeSymbol type) =>
        new()
        {
            Name = GetTypeName(type),
            StandardElementType = Classify(type),
            LineNumber = type.DeclaringSyntaxReferences
                .Select(reference => reference.GetSyntax())
                .OfType<TypeDeclarationSyntax>()
                .FirstOrDefault()?
                .GetLocation()
                .GetLineSpan().StartLinePosition.Line + 1
            ?? 0,
            IsPublic = type.DeclaredAccessibility == Accessibility.Public,
            Kind = type.TypeKind == TypeKind.Interface
                ? ArchitectureTypeKind.Interface
                : ArchitectureTypeKind.Class,
        };

    private static EvaluationContext CreateEvaluationContext(
        INamedTypeSymbol type,
        IReadOnlyCollection<INamedTypeSymbol> declaredTypes,
        CSharpCompilation compilation,
        Architecture architecture
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

        Class architectureElement = architecture.Classes
            .Concat(architecture.Interfaces)
            .Single(element => string.Equals(
                element.Name,
                GetTypeName(type),
                StringComparison.Ordinal));

        architectureElement.AnalysisIsApiController = IsApiController(type);
        architectureElement.AnalysisHasExternalBaseType = InheritsFromExternalType(type);
        architectureElement.AnalysisImplementsExternalInterface = ImplementsExternalInterface(type);
        architectureElement.AnalysisHasExternalStateDependency = HasExternalStateDependency(type);
        architectureElement.AnalysisExposesExternalResource = ExposesExternalResource(type);
        architectureElement.AnalysisUsesExternalResource = UsesExternalResource(type, compilation);
        architectureElement.AnalysisDeclaresDependencyIntent = DeclaresDependencyIntent(type);
        architectureElement.AnalysisSourceFileTopLevelClassCount = GetTopLevelClasses(declaration).Count;
        architectureElement.AnalysisIsPrimaryTopLevelClassInFile = IsPrimaryTopLevelClass(declaration);

        architectureElement.AnalysisDeclarations = type.DeclaringSyntaxReferences
            .Select(reference => reference.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .ToArray();

        architectureElement.AnalysisFilePath = sourceTree?.FilePath ?? string.Empty;
        architectureElement.AnalysisSourceCode = sourceTree?.GetText().ToString() ?? string.Empty;
        architectureElement.AnalysisProjectLineEnding = architecture.AnalysisProjectLineEnding;

        architectureElement.AnalysisDependencies = type.InstanceConstructors
            .SelectMany(constructor => constructor.Parameters)
            .Select(
                parameter => CreateTypeDependency(parameter.Type, declaredTypes))
            .Where(dependency => !dependency.IsConfigurationModel)
            .GroupBy(dependency => dependency.TypeName, StringComparer.Ordinal)
            .Select(dependencies => dependencies.First())
            .ToArray();

        architectureElement.AnalysisImplementedInterfaces = type.AllInterfaces
            .Select(GetTypeName)
            .ToArray();

        architectureElement.AnalysisContractMethodNames = type.AllInterfaces
            .SelectMany(contract => contract.GetMembers())
            .OfType<IMethodSymbol>()
            .Select(method => method.Name)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        architectureElement.AnalysisPublicMethodCallLineNumbers =
            GetPublicMethodCallLineNumbers(type, compilation);

        architectureElement.AnalysisPublicApiModelTypes = GetPublicApiModelTypes(type);

        return new EvaluationContext
        {
            ArchitectureModel = architecture,
            ArchitectureElement = architectureElement,
        };
    }

    private static IReadOnlyList<ClassDeclarationSyntax> GetTopLevelClasses(
        TypeDeclarationSyntax? declaration) =>
        declaration?.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(predicate: candidate =>
                !candidate.Ancestors().OfType<TypeDeclarationSyntax>().Any())
            .ToArray()
        ?? [];

    private static bool IsPrimaryTopLevelClass(TypeDeclarationSyntax? declaration)
    {
        if (declaration is not ClassDeclarationSyntax classDeclaration)
        {
            return false;
        }

        return GetTopLevelClasses(declaration: declaration).FirstOrDefault()?.SpanStart
            == classDeclaration.SpanStart;
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
                IsConfigurationModel =
                    IsConfigurationModel(type: dependency),
            };
        }

        INamedTypeSymbol? concreteType = ResolveConcreteType(dependency: dependency, declaredTypes: declaredTypes);

        return concreteType is null
            ? CreateReferencedTypeDependency(dependency: dependency)
            : new TypeDependency
            {
                TypeName = GetTypeName(type: concreteType),
                StandardElementType = Classify(type: concreteType),
                IsConfigurationModel =
                    IsConfigurationModel(type: dependency)
                    || IsConfigurationModel(type: concreteType),
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
            IsConfigurationModel =
                IsConfigurationModel(type: dependency),
        };
    }

    private static bool IsConfigurationModel(
        ITypeSymbol type)
    {
        string typeName = type.Name;

        string containingNamespace =
            type.ContainingNamespace?.ToDisplayString() ?? string.Empty;

        return typeName.EndsWith(
                value: "Configuration",
                comparisonType: StringComparison.Ordinal)
            || typeName.EndsWith(
                value: "ConfigurationModel",
                comparisonType: StringComparison.Ordinal)
            || containingNamespace.Contains(
                value: ".Configurations",
                comparisonType: StringComparison.Ordinal)
            || containingNamespace.EndsWith(
                value: ".Configuration",
                comparisonType: StringComparison.Ordinal);
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

        string containingAssemblyName =
            type.ContainingAssembly?.Name ?? string.Empty;

        if (
            containingAssemblyName.EndsWith(
                value: "Tests",
                comparisonType: StringComparison.Ordinal)
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
            IsHttpController(type: type)
            || IsHttpMiddleware(type: type)
        )
        {
            return StandardElementType.HttpExposure;
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

    private static bool IsHttpController(INamedTypeSymbol type) =>
        type.ContainingNamespace.ToDisplayString().Contains(
            value: ".Controllers",
            comparisonType: StringComparison.Ordinal)
        || InheritsFromTypeNamed(type: type, typeName: "ControllerBase")
        || InheritsFromTypeNamed(type: type, typeName: "ODataController")
        || type.GetAttributes().Any(attribute =>
            attribute.AttributeClass?.Name == "ApiControllerAttribute");

    private static bool IsHttpMiddleware(INamedTypeSymbol type) =>
        type.AllInterfaces.Any(contract => contract.Name == "IMiddleware")
        || type.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(IsConventionalMiddlewareMethod);

    private static bool IsConventionalMiddlewareMethod(IMethodSymbol method) =>
        method.DeclaredAccessibility == Accessibility.Public
        && method.MethodKind == MethodKind.Ordinary
        && method.Name is "Invoke" or "InvokeAsync"
        && method.Parameters.Length is 1 or 2
        && method.Parameters[0].Type.Name == "HttpContext"
        && (method.Parameters.Length == 1
            || method.Parameters[1].Type.Name == "RequestDelegate");

    private static bool InheritsFromTypeNamed(
        INamedTypeSymbol type,
        string typeName)
    {
        for (INamedTypeSymbol? current = type;
            current is not null;
            current = current.BaseType)
        {
            if (current.Name == typeName)
            {
                return true;
            }
        }

        return false;
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

    private static bool ExposesExternalResource(INamedTypeSymbol type) =>

        type.GetMembers()
            .Where(predicate: (ISymbol member) => !member.IsImplicitlyDeclared)
            .Where(
                predicate: (ISymbol member) =>
                    member.DeclaredAccessibility
                        is Accessibility.Public
                        or Accessibility.Internal
                        or Accessibility.Protected
                        or Accessibility.ProtectedOrInternal)
            .Any(
                predicate: (ISymbol member) =>
                    member switch
                    {
                        IFieldSymbol field =>
                            IsExternalResource(type: field.Type),
                        IPropertySymbol property =>
                            IsExternalResource(type: property.Type),
                        IMethodSymbol method when
                            method.MethodKind != MethodKind.Constructor =>
                            IsExternalResource(type: method.ReturnType)
                            || method.Parameters.Any(
                                predicate: (IParameterSymbol parameter) =>
                                    IsExternalResource(type: parameter.Type)),
                        _ => false,
                    });

    private static bool UsesExternalResource(
        INamedTypeSymbol type,
        CSharpCompilation compilation) =>
        type.InstanceConstructors
            .SelectMany(
                selector: constructor =>
                    constructor.Parameters)
            .Any(
                predicate: parameter =>
                    IsExternalResource(type: parameter.Type))
        || type.GetMembers()
            .OfType<IFieldSymbol>()
            .Any(
                predicate: field =>
                    IsExternalResource(type: field.Type))
        || type.DeclaringSyntaxReferences
            .Select(
                selector: reference =>
                    reference.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .SelectMany(
                selector: declaration =>
                    declaration.DescendantNodes()
                        .OfType<ObjectCreationExpressionSyntax>())
            .Any(
                predicate: creation =>
                    IsExternalResource(
                        type: compilation
                            .GetSemanticModel(
                                syntaxTree: creation.SyntaxTree)
                            .GetTypeInfo(node: creation)
                            .Type));

    private static bool IsExternalResource(ITypeSymbol? type)
    {
        if (type is IArrayTypeSymbol array)
        {
            return IsExternalResource(type: array.ElementType);
        }

        if (type is not INamedTypeSymbol namedType)
        {
            return false;
        }

        string typeName = namedType.ConstructedFrom
            .ToDisplayString();

        if (typeName is "System.Threading.Tasks.Task"
            or "System.Threading.Tasks.Task<TResult>"
            or "System.Threading.Tasks.ValueTask"
            or "System.Threading.Tasks.ValueTask<TResult>")
        {
            return namedType.TypeArguments.Any(
                predicate: argument =>
                    IsExternalResource(type: argument));
        }

        bool isExternal =
            !namedType.Locations.Any(
                predicate: (Location location) => location.IsInSource);

        bool ownsDisposableResource =
            namedType.AllInterfaces.Any(
                predicate: (INamedTypeSymbol contract) =>
                    contract.ToDisplayString() == "System.IDisposable"
                    || contract.ToDisplayString() == "System.IAsyncDisposable");

        return (isExternal && ownsDisposableResource)
            || namedType.TypeArguments.Any(
                predicate: (ITypeSymbol argument) =>
                    IsExternalResource(type: argument));
    }

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