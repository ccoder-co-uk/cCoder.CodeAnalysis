// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Coordinations.Rules;
using cCoder.CodeAnalysis.Services.Orchestrations.Rules;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Foundations.Architectures;

internal sealed class ArchitectureService(IRuleEvaluationCoordinationService ruleEvaluationCoordinationService)
    : IArchitectureService
{
    private static readonly SymbolDisplayFormat FullyQualifiedTypeFormat = new SymbolDisplayFormat(
        SymbolDisplayGlobalNamespaceStyle.Omitted,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        SymbolDisplayGenericsOptions.IncludeTypeParameters,
        SymbolDisplayMemberOptions.None,
        SymbolDisplayDelegateStyle.NameOnly,
        SymbolDisplayExtensionMethodStyle.Default,
        SymbolDisplayParameterOptions.None,
        SymbolDisplayPropertyStyle.NameOnly,
        SymbolDisplayLocalOptions.None,
        SymbolDisplayKindOptions.None,
        SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
    );

    public ArchitectureService()
        : this(CreateDefaultRuleEvaluationCoordinationService()) { }

    public Architecture Build(string projectFilePath)
    {
        string projectDirectory =
            Path.GetDirectoryName(projectFilePath)
            ?? throw new InvalidOperationException("The project path has no containing directory.");
        SyntaxTree[] projectSyntaxTrees = (
            from path in (
                from path in Directory.GetFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
                where !IsBuildOutput(path, projectDirectory)
                select path
            ).OrderBy((string path) => path, StringComparer.OrdinalIgnoreCase)
            select CSharpSyntaxTree.ParseText(File.ReadAllText(path), null, path)
        ).ToArray();
        SyntaxTree syntaxTree = CreateImplicitUsingsSyntaxTree();
        SyntaxTree[] array = projectSyntaxTrees;
        int num = 0;
        SyntaxTree[] array2 = new SyntaxTree[1 + array.Length];
        array2[num] = syntaxTree;
        num++;
        ReadOnlySpan<SyntaxTree> readOnlySpan = new ReadOnlySpan<SyntaxTree>(array);
        readOnlySpan.CopyTo(new Span<SyntaxTree>(array2).Slice(num, readOnlySpan.Length));
        num += readOnlySpan.Length;
        SyntaxTree[] compilationSyntaxTrees = array2;
        CSharpCompilation compilation = CSharpCompilation.Create(
            Path.GetFileNameWithoutExtension(projectFilePath),
            compilationSyntaxTrees,
            GetMetadataReferences(projectFilePath),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
        return Build(compilation, projectSyntaxTrees);
    }

    internal Architecture Build(CSharpCompilation compilation)
    {
        return Build(
            compilation,
            compilation.SyntaxTrees.Where((SyntaxTree syntaxTree) => syntaxTree.FilePath.Length > 0).ToArray()
        );
    }

    private Architecture Build(CSharpCompilation compilation, SyntaxTree[] projectSyntaxTrees)
    {
        INamedTypeSymbol[] declaredTypes = projectSyntaxTrees
            .SelectMany((SyntaxTree tree) => GetDeclaredTypes(compilation, tree))
            .Where(
                delegate(INamedTypeSymbol type)
                {
                    string name = type.Name;
                    return !(name == "ValidationRule") && !(name == "ValidationRulesEngine");
                }
            )
            .GroupBy(GetTypeName, StringComparer.Ordinal)
            .Select((IGrouping<string, INamedTypeSymbol> types) => types.First())
            .ToArray();
        Architecture architecture = new Architecture();
        architecture.Classes = declaredTypes
            .Where((INamedTypeSymbol type) => type.TypeKind == TypeKind.Class)
            .Select(CreateClass)
            .OrderBy<Class, string>((Class item) => item.Name, StringComparer.Ordinal)
            .ToList();
        Architecture architecture2 = architecture;
        architecture2.Links = declaredTypes
            .Where((INamedTypeSymbol type) => type.TypeKind == TypeKind.Class)
            .SelectMany((INamedTypeSymbol type) => CreateLinks(type, declaredTypes))
            .GroupBy((Link link) => (FromType: link.FromType, ToType: link.ToType))
            .Select((IGrouping<(string FromType, string ToType), Link> links) => links.First())
            .OrderBy<Link, string>((Link link) => link.FromType, StringComparer.Ordinal)
            .ThenBy<Link, string>((Link link) => link.ToType, StringComparer.Ordinal)
            .ToList();
        architecture2.AnalysisItems = ruleEvaluationCoordinationService
            .Evaluate(
                from type in declaredTypes
                where type.TypeKind == TypeKind.Class
                select CreateEvaluationContext(type, declaredTypes, compilation)
            )
            .ToList();
        return architecture2;
    }

    private static EvaluationContext CreateEvaluationContext(
        INamedTypeSymbol type,
        IReadOnlyCollection<INamedTypeSymbol> declaredTypes,
        CSharpCompilation compilation
    )
    {
        TypeDeclarationSyntax? declaration = type
            .DeclaringSyntaxReferences.Select((SyntaxReference reference) => reference.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault();
        EvaluationContext evaluationContext = new EvaluationContext();
        evaluationContext.TypeName = GetTypeName(type);
        evaluationContext.StandardElementType = Classify(type);
        evaluationContext.LineNumber = (
            (declaration != null) ? (declaration.GetLocation().GetLineSpan().StartLinePosition.Line + 1) : 0
        );
        evaluationContext.IsPublic = type.DeclaredAccessibility == Accessibility.Public;
        evaluationContext.IsApiController = type
            .ContainingNamespace.ToDisplayString()
            .Contains(".Controllers", StringComparison.Ordinal);
        evaluationContext.HasBaseClass =
            type.BaseType != null
            && type.BaseType.SpecialType != SpecialType.System_Object;
        evaluationContext.Declarations = type
            .DeclaringSyntaxReferences.Select((SyntaxReference reference) => reference.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .ToArray();
        evaluationContext.Dependencies = (
            from parameter in type.InstanceConstructors.SelectMany(
                (IMethodSymbol constructor) => constructor.Parameters
            )
            select CreateTypeDependency(parameter.Type, declaredTypes)
        )
            .GroupBy((TypeDependency dependency) => dependency.TypeName, StringComparer.Ordinal)
            .Select((IGrouping<string, TypeDependency> dependencies) => dependencies.First())
            .ToArray();
        evaluationContext.ImplementedInterfaces = type.AllInterfaces.Select(GetTypeName).ToArray();
        evaluationContext.PublicMethodNames = (
            from method in type.GetMembers().OfType<IMethodSymbol>()
            where method.MethodKind == MethodKind.Ordinary && method.DeclaredAccessibility == Accessibility.Public
            select method.Name
        )
            .Distinct<string>(StringComparer.Ordinal)
            .ToArray();
        evaluationContext.ContractMethodNames = (
            from method in type
                .AllInterfaces.SelectMany((INamedTypeSymbol contract) => contract.GetMembers())
                .OfType<IMethodSymbol>()
            select method.Name
        )
            .Distinct<string>(StringComparer.Ordinal)
            .ToArray();
        evaluationContext.PublicMethodCallLineNumbers = GetPublicMethodCallLineNumbers(type, compilation);
        evaluationContext.PublicApiModelTypes = GetPublicApiModelTypes(type);
        return evaluationContext;
    }

    private static string[] GetPublicApiModelTypes(INamedTypeSymbol type)
    {
        return (
            from modelType in (
                from method in type.GetMembers().OfType<IMethodSymbol>()
                where method.MethodKind == MethodKind.Ordinary && method.DeclaredAccessibility == Accessibility.Public
                select method
            )
                .SelectMany(
                    (IMethodSymbol method) =>
                        method
                            .Parameters.Select((IParameterSymbol parameter) => parameter.Type)
                            .Append(method.ReturnType)
                )
                .SelectMany(GetContainedNamedTypes)
            where Classify(modelType) == StandardElementType.Model
            select GetTypeName(modelType).TrimEnd('?')
        )
            .Distinct<string>(StringComparer.Ordinal)
            .ToArray();
    }

    private static IEnumerable<INamedTypeSymbol> GetContainedNamedTypes(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol arrayType)
        {
            return GetContainedNamedTypes(arrayType.ElementType);
        }
        if (!(type is INamedTypeSymbol namedType))
        {
            return Array.Empty<INamedTypeSymbol>();
        }
        return namedType.TypeArguments.SelectMany(GetContainedNamedTypes).Prepend(namedType);
    }

    private static int[] GetPublicMethodCallLineNumbers(INamedTypeSymbol type, CSharpCompilation compilation)
    {
        return (
            from invocation in type
                .DeclaringSyntaxReferences.Select((SyntaxReference reference) => reference.GetSyntax())
                .OfType<TypeDeclarationSyntax>()
                .SelectMany((TypeDeclarationSyntax declaration) => declaration.DescendantNodes())
                .OfType<InvocationExpressionSyntax>()
            where IsPublicMethodCallOnSameType(invocation, type, compilation)
            select invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1
        ).ToArray();
    }

    private static bool IsPublicMethodCallOnSameType(
        InvocationExpressionSyntax invocation,
        INamedTypeSymbol type,
        CSharpCompilation compilation
    )
    {
        SemanticModel semanticModel = compilation.GetSemanticModel(invocation.SyntaxTree);
        MethodDeclarationSyntax? containingMethod = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        IMethodSymbol? caller = containingMethod is null ? null : semanticModel.GetDeclaredSymbol(containingMethod);
        return caller != null
            && caller.DeclaredAccessibility == Accessibility.Public
            && semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol calledMethod
            && calledMethod.DeclaredAccessibility == Accessibility.Public
            && SymbolEqualityComparer.Default.Equals(calledMethod.ContainingType, type);
    }

    private static TypeDependency CreateTypeDependency(
        ITypeSymbol dependency,
        IReadOnlyCollection<INamedTypeSymbol> declaredTypes
    )
    {
        INamedTypeSymbol? concreteType = ResolveConcreteType(dependency, declaredTypes);
        return (concreteType == null)
            ? CreateReferencedTypeDependency(dependency)
            : new TypeDependency { TypeName = GetTypeName(concreteType), StandardElementType = Classify(concreteType) };
    }

    private static TypeDependency CreateReferencedTypeDependency(ITypeSymbol dependency)
    {
        StandardElementType elementType = (
            (dependency is INamedTypeSymbol namedType) ? Classify(namedType) : StandardElementType.Unknown
        );
        return new TypeDependency
        {
            TypeName = GetTypeName(dependency),
            StandardElementType = (
                (elementType == StandardElementType.Unknown) ? StandardElementType.Dependency : elementType
            ),
        };
    }

    private static IEnumerable<INamedTypeSymbol> GetDeclaredTypes(CSharpCompilation compilation, SyntaxTree syntaxTree)
    {
        SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
        return (
            from declaration in syntaxTree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>()
            select semanticModel.GetDeclaredSymbol(declaration)
        ).OfType<INamedTypeSymbol>();
    }

    private static Class CreateClass(INamedTypeSymbol type)
    {
        return new Class
        {
            Name = GetTypeName(type),
            StandardElementType = Classify(type),
            Properties = (
                from property in type.GetMembers().OfType<IPropertySymbol>()
                where property.DeclaredAccessibility == Accessibility.Public
                select new Property { Name = property.Name, Type = GetTypeName(property.Type) }
            )
                .OrderBy<Property, string>((Property property) => property.Name, StringComparer.Ordinal)
                .ToList(),
            Methods = (
                from method in type.GetMembers().OfType<IMethodSymbol>()
                where method.MethodKind == MethodKind.Ordinary && method.DeclaredAccessibility == Accessibility.Public
                select new Method
                {
                    Name = method.Name,
                    ReturnType = GetTypeName(method.ReturnType),
                    Inputs = method
                        .Parameters.Select(
                            (IParameterSymbol parameter) =>
                                new Input { Name = parameter.Name, Type = GetTypeName(parameter.Type) }
                        )
                        .ToList(),
                }
            )
                .OrderBy<Method, string>((Method method) => method.Name, StringComparer.Ordinal)
                .ToList(),
        };
    }

    private static IEnumerable<Link> CreateLinks(
        INamedTypeSymbol type,
        IReadOnlyCollection<INamedTypeSymbol> declaredTypes
    )
    {
        IEnumerable<ITypeSymbol> dependencies =
            from parameter in type.InstanceConstructors.SelectMany(
                (IMethodSymbol constructor) => constructor.Parameters
            )
            select parameter.Type;
        foreach (ITypeSymbol dependency in dependencies)
        {
            INamedTypeSymbol? target = ResolveConcreteType(dependency, declaredTypes);
            if (target != null)
            {
                yield return new Link { FromType = GetTypeName(type), ToType = GetTypeName(target) };
            }
        }
    }

    private static INamedTypeSymbol? ResolveConcreteType(
        ITypeSymbol dependency,
        IReadOnlyCollection<INamedTypeSymbol> declaredTypes
    )
    {
        if (dependency.TypeKind == TypeKind.Class && declaredTypes.Contains(dependency, SymbolEqualityComparer.Default))
        {
            return (INamedTypeSymbol)dependency;
        }
        return declaredTypes.SingleOrDefault(
            (INamedTypeSymbol type) =>
                type.TypeKind == TypeKind.Class
                && type.AllInterfaces.Contains(dependency, SymbolEqualityComparer.Default)
        );
    }

    private static StandardElementType Classify(INamedTypeSymbol type)
    {
        string containingNamespace = type.ContainingNamespace.ToDisplayString();
        if (
            type.ContainingAssembly.Name.EndsWith("Tests", StringComparison.Ordinal)
            || containingNamespace.Contains(".Tests", StringComparison.Ordinal)
        )
        {
            return StandardElementType.Test;
        }
        if (type.Name == "Program")
        {
            return StandardElementType.Exposure;
        }
        if (containingNamespace.Contains(".Controllers", StringComparison.Ordinal))
        {
            return StandardElementType.Exposure;
        }
        if (containingNamespace.Contains(".Dependencies", StringComparison.Ordinal))
        {
            return StandardElementType.Dependency;
        }
        if (containingNamespace.Contains(".Migrations", StringComparison.Ordinal))
        {
            return StandardElementType.Dependency;
        }
        if (
            (
                containingNamespace.Contains(".Brokers", StringComparison.Ordinal)
                || containingNamespace.Contains(".Exposures", StringComparison.Ordinal)
            ) && InheritsFromExternalType(type)
        )
        {
            return StandardElementType.Dependency;
        }
        if (type.Name == "IServiceCollectionExtensions")
        {
            return StandardElementType.Exposure;
        }
        if (
            type.Name.EndsWith("EventHub", StringComparison.Ordinal)
            || type.Name == "WebApplicationExtensions"
        )
        {
            return StandardElementType.Exposure;
        }
        if (
            type.Name == "EventProvider"
            || type.Name == "BulkEventProvider"
        )
        {
            return StandardElementType.Dependency;
        }
        if (containingNamespace.Contains(".Exposures", StringComparison.Ordinal))
        {
            return StandardElementType.Exposure;
        }
        if (containingNamespace.Contains(".Services.Foundations", StringComparison.Ordinal))
        {
            return StandardElementType.FoundationService;
        }
        if (containingNamespace.Contains(".Services.Processings", StringComparison.Ordinal))
        {
            return StandardElementType.ProcessingService;
        }
        if (containingNamespace.Contains(".Services.Orchestrations", StringComparison.Ordinal))
        {
            return StandardElementType.OrchestrationService;
        }
        if (containingNamespace.Contains(".Services.Coordinations", StringComparison.Ordinal))
        {
            return StandardElementType.CoordinationService;
        }
        if (containingNamespace.Contains(".Services.Managements", StringComparison.Ordinal))
        {
            return StandardElementType.ManagementService;
        }
        if (containingNamespace.Contains(".Services.Aggregations", StringComparison.Ordinal))
        {
            return StandardElementType.AggregationService;
        }
        if (containingNamespace.Contains(".Models", StringComparison.Ordinal))
        {
            return StandardElementType.Model;
        }
        if (containingNamespace.Contains(".Brokers", StringComparison.Ordinal))
        {
            return StandardElementType.Broker;
        }
        return StandardElementType.Unknown;
    }

    private static bool InheritsFromExternalType(INamedTypeSymbol type)
    {
        INamedTypeSymbol? baseType = type.BaseType;
        return baseType != null
            && baseType.SpecialType != SpecialType.System_Object
            && !baseType.Locations.Any((Location location) => location.IsInSource);
    }

    private static string GetTypeName(ITypeSymbol type)
    {
        return type.ToDisplayString(FullyQualifiedTypeFormat);
    }

    private static IEnumerable<MetadataReference> GetMetadataReferences(string projectFilePath)
    {
        string trustedAssemblies =
            (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string)
            ?? throw new InvalidOperationException("Platform assemblies could not be resolved.");
        IEnumerable<string> platformAssemblies = trustedAssemblies.Split(Path.PathSeparator).AsEnumerable();
        IEnumerable<string> buildAssemblies = GetBuildAssemblies(projectFilePath);
        return from path in platformAssemblies
                .Concat(buildAssemblies)
                .Distinct<string>(StringComparer.OrdinalIgnoreCase)
            select MetadataReference.CreateFromFile(path);
    }

    private static string[] GetBuildAssemblies(string projectFilePath)
    {
        string projectDirectory =
            Path.GetDirectoryName(projectFilePath)
            ?? throw new InvalidOperationException("The project path has no containing directory.");
        string projectName = Path.GetFileNameWithoutExtension(projectFilePath);
        string buildDirectory = Path.Combine(projectDirectory, "bin");
        if (!Directory.Exists(buildDirectory))
        {
            return Array.Empty<string>();
        }
        string? projectAssembly = Directory
            .GetFiles(buildDirectory, projectName + ".dll", SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
        return projectAssembly == null
            ? Array.Empty<string>()
            : Directory.GetFiles(Path.GetDirectoryName(projectAssembly)!, "*.dll", SearchOption.TopDirectoryOnly);
    }

    private static SyntaxTree CreateImplicitUsingsSyntaxTree()
    {
        return CSharpSyntaxTree.ParseText(
            "global using System;\r\nglobal using System.Collections.Generic;\r\nglobal using System.IO;\r\nglobal using System.Linq;\r\nglobal using System.Threading;\r\nglobal using System.Threading.Tasks;"
        );
    }

    private static bool IsBuildOutput(string path, string projectDirectory)
    {
        string projectDirectoryPrefix = Path
            .GetFullPath(projectDirectory)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string fullPath = Path.GetFullPath(path);
        string relativePath = fullPath.StartsWith(projectDirectoryPrefix, StringComparison.OrdinalIgnoreCase)
            ? fullPath.Substring(projectDirectoryPrefix.Length)
            : fullPath;
        return relativePath.StartsWith($"bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
            || relativePath.StartsWith($"obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
    }

    private static RuleEvaluationCoordinationService CreateDefaultRuleEvaluationCoordinationService()
    {
        BrokerCodeAnalysisRulesProcessingService brokerRules = new BrokerCodeAnalysisRulesProcessingService();
        FoundationServiceCodeAnalysisRulesProcessingService foundationRules =
            new FoundationServiceCodeAnalysisRulesProcessingService();
        DependencyCodeAnalysisRulesProcessingService dependencyRules =
            new DependencyCodeAnalysisRulesProcessingService();
        ProcessingServiceCodeAnalysisRulesProcessingService processingRules =
            new ProcessingServiceCodeAnalysisRulesProcessingService();
        OrchestrationServiceCodeAnalysisRulesProcessingService orchestrationRules =
            new OrchestrationServiceCodeAnalysisRulesProcessingService();
        CoordinationServiceCodeAnalysisRulesProcessingService coordinationRules =
            new CoordinationServiceCodeAnalysisRulesProcessingService();
        ManagementServiceCodeAnalysisRulesProcessingService managementRules =
            new ManagementServiceCodeAnalysisRulesProcessingService();
        AggregationServiceCodeAnalysisRulesProcessingService aggregationRules =
            new AggregationServiceCodeAnalysisRulesProcessingService();
        ExposureCodeAnalysisRulesProcessingService exposureRules = new ExposureCodeAnalysisRulesProcessingService();
        ModelCodeAnalysisRulesProcessingService modelRules = new ModelCodeAnalysisRulesProcessingService();
        TestCodeAnalysisRulesProcessingService testRules = new TestCodeAnalysisRulesProcessingService();
        CulDeSacServicesAndBrokerRuleEvaluationOrchestrationService culDeSacRules =
            new CulDeSacServicesAndBrokerRuleEvaluationOrchestrationService(
                brokerRules,
                foundationRules,
                dependencyRules
            );
        HigherLevelServicesRuleEvaluationOrchestrationService higherLevelRules =
            new HigherLevelServicesRuleEvaluationOrchestrationService(
                processingRules,
                orchestrationRules,
                coordinationRules,
                managementRules,
                aggregationRules
            );
        ExposuresAndModelsRuleEvaluationOrchestrationService exposuresAndModelsRules =
            new ExposuresAndModelsRuleEvaluationOrchestrationService(exposureRules, modelRules, testRules);
        return new RuleEvaluationCoordinationService(culDeSacRules, higherLevelRules, exposuresAndModelsRules);
    }
}
