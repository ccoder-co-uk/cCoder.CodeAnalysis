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
            Project = new ProjectMetadata
            {
                Id = compilation.AssemblyName ?? string.Empty,
                Name = compilation.AssemblyName ?? string.Empty,
                AssemblyName = compilation.AssemblyName ?? string.Empty,
            },
            Classes = declaredTypes
                .Where(predicate: (INamedTypeSymbol type) => type.TypeKind == TypeKind.Class)
            .Select(
                selector: (INamedTypeSymbol type) =>
                    CreateClass(
                        type: type,
                        compilation: compilation,
                        declaredTypes: declaredTypes))
                .OrderBy(keySelector: (Class item) => item.Name, comparer: StringComparer.Ordinal)
                .ToList(),
            Interfaces = declaredTypes
                .Where(predicate: (INamedTypeSymbol type) => type.TypeKind == TypeKind.Interface)
            .Select(
                selector: (INamedTypeSymbol type) =>
                    CreateClass(
                        type: type,
                        compilation: compilation,
                        declaredTypes: declaredTypes))
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
                ".generated.cs",
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

        int headerLength = Math.Min(
            val1: source.Length,
            val2: 500);

        return source
            .Substring(
                startIndex: 0,
                length: headerLength)
            .Contains(
                "<auto-generated",
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

    private static Class CreateClass(
        INamedTypeSymbol type,
        CSharpCompilation compilation,
        IReadOnlyCollection<INamedTypeSymbol> declaredTypes)
    {
        List<Method> analysisMethods = type
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(predicate: (IMethodSymbol method) => method.MethodKind == MethodKind.Ordinary)
            .Select(
                selector: (IMethodSymbol method) =>
                    CreateMethod(
                        method: method,
                        compilation: compilation,
                        declaredTypes: declaredTypes))
            .OrderBy(keySelector: (Method method) => method.Name, comparer: StringComparer.Ordinal)
            .ThenBy(keySelector: (Method method) => method.Id, comparer: StringComparer.Ordinal)
            .ToList();

        return new Class
        {
            Name = GetTypeName(type: type),
            StandardElementType = Classify(type: type),
            LineNumber = GetDeclarationLineNumber(type: type),
            IsPublic = type.DeclaredAccessibility == Accessibility.Public,
            Kind = GetArchitectureTypeKind(type: type),
            BaseType = type.BaseType is not null
                && type.BaseType.SpecialType != SpecialType.System_Object
                    ? CreateTypeReference(
                        type: type.BaseType,
                        compilation: compilation,
                        declaredTypes: declaredTypes)
                    : null,
            Interfaces = type.Interfaces
                .Select(selector: contract =>
                    CreateTypeReference(
                        type: contract,
                        compilation: compilation,
                        declaredTypes: declaredTypes))
                .OrderBy(keySelector: reference => reference.Id, comparer: StringComparer.Ordinal)
                .ToList(),
            Properties = type.GetMembers()
            .OfType<IPropertySymbol>()
                .Where(predicate: (IPropertySymbol property) => property.DeclaredAccessibility == Accessibility.Public)
                .Select(
                    selector: (IPropertySymbol property) =>
                        new Property { Name = property.Name, Type = GetTypeName(type: property.Type) }
                )
                .OrderBy(keySelector: (Property property) => property.Name, comparer: StringComparer.Ordinal)
                .ToList(),
            Methods = analysisMethods
                .Where(predicate: (Method method) => method.Symbol.DeclaredAccessibility == Accessibility.Public)
                .ToList(),
            AnalysisMethods = analysisMethods,
            AnalysisImplementedInterfaces = type.AllInterfaces
                .Select(selector: GetTypeName)
                .OrderBy(keySelector: interfaceName => interfaceName, comparer: StringComparer.Ordinal)
                .ToArray(),
            AnalysisTypeFacts = CreateTypeAnalysisFacts(
                type: type,
                compilation: compilation,
                declaredTypes: declaredTypes),
        };
    }

    private static int GetDeclarationLineNumber(INamedTypeSymbol type) =>
        type.DeclaringSyntaxReferences
            .Select(reference => reference.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault()?
            .GetLocation()
            .GetLineSpan().StartLinePosition.Line + 1
        ?? 0;

    private static ArchitectureTypeKind GetArchitectureTypeKind(
        INamedTypeSymbol type) =>
        type.TypeKind == TypeKind.Interface
            ? ArchitectureTypeKind.Interface
            : ArchitectureTypeKind.Class;

    private static TypeReference CreateTypeReference(
        INamedTypeSymbol type,
        CSharpCompilation compilation,
        IReadOnlyCollection<INamedTypeSymbol> declaredTypes)
    {
        string assemblyName = type.ContainingAssembly?.Name ?? string.Empty;
        string fullName = GetTypeName(type: type);

        bool isInCurrentProject = SymbolEqualityComparer.Default.Equals(
            x: type.ContainingAssembly,
            y: compilation.Assembly);

        INamedTypeSymbol? declaredType = declaredTypes.FirstOrDefault(
            predicate: candidate =>
                SymbolEqualityComparer.Default.Equals(
                    x: candidate,
                    y: type)
                || SymbolEqualityComparer.Default.Equals(
                    x: candidate,
                    y: type.OriginalDefinition));

        return new TypeReference
        {
            Id = $"{assemblyName}:{fullName}",
            FullName = fullName,
            Name = type.Name,
            Namespace = type.ContainingNamespace?.ToDisplayString() ?? string.Empty,
            AssemblyName = assemblyName,
            Kind = GetArchitectureTypeKind(type: type),
            IsInCurrentProject = isInCurrentProject,
            StandardElementType = declaredType is null
                ? StandardElementType.Dependency
                : Classify(type: declaredType),
        };
    }

    private static TypeAnalysisFacts CreateTypeAnalysisFacts(
        INamedTypeSymbol type,
        CSharpCompilation compilation,
        IReadOnlyCollection<INamedTypeSymbol> declaredTypes)
    {
        TypeAnalysisFacts facts = CreateTypeAnalysisFacts(type.DeclaringSyntaxReferences
            .Select(reference => reference.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .ToArray());

        SyntaxTree? syntaxTree = type.DeclaringSyntaxReferences
            .Select(reference => reference.SyntaxTree)
            .FirstOrDefault();

        facts.ProjectName = compilation.AssemblyName ?? string.Empty;
        facts.FilePath = syntaxTree?.FilePath ?? string.Empty;
        facts.SourceCode = syntaxTree?.GetText().ToString() ?? string.Empty;

        facts.IsConsoleApplication = compilation.Options.OutputKind
            is OutputKind.ConsoleApplication or OutputKind.WindowsApplication;

        facts.ProjectTypeNames = declaredTypes.Select(GetTypeName).ToArray();

        return facts;
    }

    internal static TypeAnalysisFacts CreateTypeAnalysisFacts(
        IReadOnlyList<TypeDeclarationSyntax> declarations)
    {

        MethodAnalysisFacts[] methods = declarations
            .SelectMany(declaration => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .Select(CreateMethodAnalysisFacts)
            .ToArray();

        PropertyAnalysisFacts[] properties = declarations
            .SelectMany(declaration => declaration.Members)
            .OfType<PropertyDeclarationSyntax>()
            .Select(property => new PropertyAnalysisFacts
            {
                TypeName = property.Type.ToString(),
                LineNumber = GetLineNumber(property),
                IsPublic = property.Modifiers.Any(SyntaxKind.PublicKeyword),
                HasGetter = property.AccessorList?.Accessors.Any(
                    accessor => accessor.IsKind(SyntaxKind.GetAccessorDeclaration)) == true,
                HasSetter = property.AccessorList?.Accessors.Any(
                    accessor => accessor.IsKind(SyntaxKind.SetAccessorDeclaration)) == true,
            })
            .ToArray();

        TypeDeclarationSyntax? firstWithBaseType = declarations.FirstOrDefault(
            declaration => declaration.BaseList is not null);

        TypeDeclarationSyntax? firstNonPartial = declarations.FirstOrDefault(
            declaration => !declaration.Modifiers.Any(SyntaxKind.PartialKeyword));

        return new TypeAnalysisFacts
        {
            Methods = methods,
            Properties = properties,
            AllDeclarationsArePartial = declarations.All(
                declaration => declaration.Modifiers.Any(SyntaxKind.PartialKeyword)),
            FirstNonPartialDeclarationLine = firstNonPartial is null
                ? 0
                : GetLineNumber(firstNonPartial),
            BaseTypeLine = firstWithBaseType?.BaseList is null
                ? 0
                : GetLineNumber(firstWithBaseType.BaseList),
            BranchingLineNumbers = declarations
                .SelectMany(declaration => declaration.DescendantNodes())
                .Where(node => node is IfStatementSyntax
                    or SwitchStatementSyntax
                    or ConditionalExpressionSyntax)
                .Select(GetLineNumber)
                .ToArray(),
            MvcActionResponseBranchingLineNumbers = declarations
                .SelectMany(declaration => declaration.DescendantNodes())
                .Where(node => node is IfStatementSyntax
                    or SwitchStatementSyntax
                    or ConditionalExpressionSyntax)
                .Where(node => node.Ancestors()
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault() is MethodDeclarationSyntax containingMethod
                    && IsMvcActionResponseMethod(method: containingMethod))
                .Select(GetLineNumber)
                .ToArray(),
            LoopLineNumbers = declarations
                .SelectMany(declaration => declaration.DescendantNodes())
                .Where(node => node is ForStatementSyntax
                    or ForEachStatementSyntax
                    or WhileStatementSyntax
                    or DoStatementSyntax)
                .Select(GetLineNumber)
                .ToArray(),
        };
    }

    private static MethodAnalysisFacts CreateMethodAnalysisFacts(
        MethodDeclarationSyntax method)
    {
        InvocationExpressionSyntax[] invocations = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .ToArray();

        ParameterSyntax? commandParameter = method.ParameterList.Parameters
            .FirstOrDefault(parameter =>
                parameter.Type?.ToString() is "string" or "string[]" or "IReadOnlyList<string>");

        string source = method.ToFullString();
        int given = source.IndexOf("// Given", StringComparison.Ordinal);
        int when = source.IndexOf("// When", StringComparison.Ordinal);
        int then = source.IndexOf("// Then", StringComparison.Ordinal);

        string[] attributes = method.AttributeLists
            .SelectMany(attributes => attributes.Attributes)
            .Select(attribute => attribute.Name.ToString())
            .ToArray();

        ParameterSyntax? extensionParameter = method.ParameterList.Parameters.FirstOrDefault();
        bool isMvcActionResponse = IsMvcActionResponseMethod(method: method);

        return new MethodAnalysisFacts
        {
            Name = method.Identifier.Text,
            LineNumber = GetLineNumber(method),
            IsPublic = method.Modifiers.Any(SyntaxKind.PublicKeyword),
            IsPrivate = method.Modifiers.Any(SyntaxKind.PrivateKeyword),
            IsGeneric = method.TypeParameterList is not null,
            IsTest = attributes.Any(attribute =>
                attribute is "Fact" or "FactAttribute" or "Theory" or "TheoryAttribute"),
            IsFact = attributes.Any(attribute => attribute is "Fact" or "FactAttribute"),
            HasGivenWhenThenComments = given >= 0 && when > given && then > when,
            HasInvocations = invocations.Length > 0,
            HasServiceCollectionParameter = method.ParameterList.Parameters.Any(
                parameter => parameter.Type?.ToString() == "IServiceCollection"),
            FirstParameterIsServiceCollectionExtension = method.ParameterList.Parameters
                .FirstOrDefault() is ParameterSyntax serviceParameter
                && serviceParameter.Type?.ToString() == "IServiceCollection"
                && serviceParameter.Modifiers.Any(SyntaxKind.ThisKeyword),
            HasConfigurationParameter = method.ParameterList.Parameters.Any(
                parameter => parameter.Type?.ToString() == "IConfiguration"),
            ConfigurationCallbackType = GetConfigurationCallbackType(method),
            HasCommandDetailsParameter = commandParameter is not null,
            ResolvesServiceFromProvider = invocations.Any(invocation =>
                invocation.Expression.ToString().Contains("GetRequiredService", StringComparison.Ordinal)
                || invocation.Expression.ToString().Contains("GetService", StringComparison.Ordinal)),
            PassesCommandDetails = commandParameter is not null
                && invocations.Any(invocation => invocation.ArgumentList.Arguments.Any(
                    argument => argument.Expression.ToString() == commandParameter.Identifier.Text)),
            HasChainedServiceCollectionRegistration = invocations.Any(
                IsChainedServiceCollectionRegistration),
            IsExtensionMethod = extensionParameter is not null
                && extensionParameter.Modifiers.Any(SyntaxKind.ThisKeyword),
            ExtensionReceiverTypeName = extensionParameter?.Type?.ToString() ?? string.Empty,
            HasMultipleRoutineCallStatements = method.Body is not null
                && !isMvcActionResponse
                && method.Body.Statements.Count(statement =>
                    statement.DescendantNodesAndSelf()
                        .OfType<InvocationExpressionSyntax>()
                        .Any()) > 1,
            IsMvcActionResponse = isMvcActionResponse,
            HasScopedOrTransientConfigurationRegistration = invocations.Any(invocation =>
                invocation.ToString().Contains("Configuration", StringComparison.Ordinal)
                && (invocation.Expression.ToString().Contains("AddScoped", StringComparison.Ordinal)
                    || invocation.Expression.ToString().Contains("AddTransient", StringComparison.Ordinal))),
            InvokedMethodNames = invocations.Select(invocation => invocation.Expression switch
                {
                    IdentifierNameSyntax identifier => identifier.Identifier.Text,
                    MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
                    _ => string.Empty,
                })
                .Where(name => name.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
        };
    }

    private static bool IsMvcActionResponseMethod(MethodDeclarationSyntax method) =>
        method.Modifiers.Any(SyntaxKind.PublicKeyword)
        && method.ReturnType.ToString().Contains(
            value: "IActionResult",
            comparisonType: StringComparison.Ordinal);

    private static string GetConfigurationCallbackType(
        MethodDeclarationSyntax method)
    {
        TypeSyntax? callbackType = method.ParameterList.Parameters
            .Select(parameter => parameter.Type)
            .FirstOrDefault(type => type switch
            {
                GenericNameSyntax generic => generic.Identifier.Text == "Action",
                NullableTypeSyntax { ElementType: GenericNameSyntax generic } =>
                    generic.Identifier.Text == "Action",
                _ => false,
            });

        GenericNameSyntax? action = callbackType switch
        {
            GenericNameSyntax generic => generic,
            NullableTypeSyntax { ElementType: GenericNameSyntax generic } => generic,
            _ => null,
        };

        return action?.TypeArgumentList.Arguments.Count == 1
            ? action.TypeArgumentList.Arguments[0].ToString()
            : string.Empty;
    }

    private static bool IsChainedServiceCollectionRegistration(
        InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess
            || memberAccess.Expression is not InvocationExpressionSyntax previousInvocation
            || !memberAccess.Name.Identifier.Text.StartsWith("Add", StringComparison.Ordinal))
        {
            return false;
        }

        ExpressionSyntax expression = previousInvocation.Expression;

        while (expression is MemberAccessExpressionSyntax nestedMemberAccess)
        {
            expression = nestedMemberAccess.Expression is InvocationExpressionSyntax nestedInvocation
                ? nestedInvocation.Expression
                : nestedMemberAccess.Expression;
        }

        return expression is IdentifierNameSyntax identifier
            && identifier.Identifier.Text == "services";
    }

    private static int GetLineNumber(SyntaxNode node) =>
        node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

    private static Method CreateMethod(
        IMethodSymbol method,
        CSharpCompilation compilation,
        IReadOnlyCollection<INamedTypeSymbol> declaredTypes)
    {
        List<MethodCall> directCalls = GetDirectCalls(
            method: method,
            compilation: compilation,
            declaredTypes: declaredTypes);

        List<string> directlyThrownExceptionTypes = GetDirectlyThrownExceptionTypes(
            method: method,
            compilation: compilation);

        if (HasEntityFrameworkSaveChanges(method: method, compilation: compilation))
        {
            directlyThrownExceptionTypes.Add(
                "Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException");
        }

        directlyThrownExceptionTypes = directlyThrownExceptionTypes
            .Distinct(StringComparer.Ordinal)
            .OrderBy(typeName => typeName, StringComparer.Ordinal)
            .ToList();

        List<string> httpMethods = GetHttpMethods(method: method);

        bool isODataControllerAction = InheritsFromTypeNamed(
            type: method.ContainingType,
            typeName: "ODataController");

        bool isHttpRequestHandler = httpMethods.Count > 0
            || isODataControllerAction
            || IsHttpController(type: method.ContainingType)
            || IsConventionalMiddlewareMethod(method: method);

        List<HttpResponse> httpResponses = isHttpRequestHandler
            ? GetHttpResponses(method: method, compilation: compilation)
            : [];

        List<ExceptionCatch> exceptionCatches = GetExceptionCatches(
            method: method,
            compilation: compilation);

        return new Method
        {
            Id = GetMethodId(method: method),
            Name = method.Name,
            LineNumber = method.Locations
                .FirstOrDefault(location => location.IsInSource)?
                .GetLineSpan()
                .StartLinePosition.Line + 1
                ?? 0,
            ReturnType = GetTypeName(type: method.ReturnType),
            Inputs = method
                .Parameters.Select(
                    selector: (IParameterSymbol parameter) =>
                        new Input { Name = parameter.Name, Type = GetTypeName(type: parameter.Type) })
                .ToList(),
            Implements = GetImplementedMethodIds(method: method),
            Calls = directCalls
                .Where(
                    predicate: (MethodCall call) =>
                        call.IsDependencyBoundary
                        || (
                            call.TargetSymbol.MethodKind == MethodKind.Ordinary
                            && (
                                call.TargetSymbol.DeclaredAccessibility == Accessibility.Public
                                || call.TargetSymbol.ContainingType.TypeKind == TypeKind.Interface)))
                .ToList(),
            PossibleExceptionTypes = directlyThrownExceptionTypes.ToList(),
            IncomingExceptionTypes = directlyThrownExceptionTypes.ToList(),
            ThrowsExceptionTypes = directlyThrownExceptionTypes.ToList(),
            HttpMethods = httpMethods,
            HttpResponses = httpResponses,
            IsHttpRequestHandler = isHttpRequestHandler,
            IsODataControllerAction = isODataControllerAction,
            HasFromBodyParameter = method.Parameters.Any(parameter =>
                parameter.GetAttributes().Any(attribute =>
                    attribute.AttributeClass?.Name == "FromBodyAttribute")),
            HasKeyParameter = isODataControllerAction
                && httpMethods.Contains("GET", StringComparer.Ordinal)
                && method.Name == "Get"
                && method.Parameters.Any(parameter =>
                    parameter.Name.Equals("key", StringComparison.OrdinalIgnoreCase)),
            HandlesNullWithNotFound = httpResponses.Any(
                response => response.StatusCode == 404 && response.IsNullPath),
            HasTryCatch = exceptionCatches.Count > 0,
            Symbol = method,
            DirectCalls = directCalls,
            DirectlyThrowsExceptionTypes = directlyThrownExceptionTypes,
            ExceptionCatches = exceptionCatches,
        };
    }

    private static bool HasEntityFrameworkSaveChanges(
        IMethodSymbol method,
        CSharpCompilation compilation) =>

        method.DeclaringSyntaxReferences
            .Select(reference => reference.GetSyntax())
            .SelectMany(declaration => declaration.DescendantNodes())
            .OfType<InvocationExpressionSyntax>()
            .Where(invocation => GetInvocationName(invocation: invocation)
                is "SaveChanges" or "SaveChangesAsync")
            .Any(invocation => IsDbContextInvocation(
                invocation: invocation,
                compilation: compilation));

    private static bool IsDbContextInvocation(
        InvocationExpressionSyntax invocation,
        CSharpCompilation compilation)
    {
        SemanticModel semanticModel = compilation.GetSemanticModel(invocation.SyntaxTree);
        IMethodSymbol? method = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

        if (method is not null
            && InheritsFromTypeNamed(type: method.ContainingType, typeName: "DbContext"))
        {
            return true;
        }

        return invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && semanticModel.GetTypeInfo(memberAccess.Expression).Type is INamedTypeSymbol receiverType
            && InheritsFromTypeNamed(type: receiverType, typeName: "DbContext");
    }

    private static List<MethodCall> GetDirectCalls(
        IMethodSymbol method,
        CSharpCompilation compilation,
        IReadOnlyCollection<INamedTypeSymbol> declaredTypes)
    {
        return method
            .DeclaringSyntaxReferences.Select(reference => reference.GetSyntax())
            .SelectMany(
                node => node.DescendantNodes().Where(
                    descendant => descendant
                        is InvocationExpressionSyntax
                        or ObjectCreationExpressionSyntax
                        or ImplicitObjectCreationExpressionSyntax))
            .Select(call => new
            {
                Node = call,
                Method = GetCalledMethod(compilation: compilation, call: call)?.OriginalDefinition,
            })
            .Where(call => call.Method is not null)
            .GroupBy(call => GetMethodId(method: call.Method!), StringComparer.Ordinal)
            .Select(group => group.First())
            .Select(
                call =>
                {
                    IMethodSymbol target = call.Method!;

                    bool isDependencyBoundary = !IsDeclaredInCurrentProject(
                        method: target,
                        compilation: compilation);

                    INamedTypeSymbol? localType = declaredTypes.FirstOrDefault(
                        type => SymbolEqualityComparer.Default.Equals(type, target.ContainingType));

                    return new MethodCall
                    {
                        TypeName = GetTypeName(type: target.ContainingType),
                        MethodName = target.Name,
                        MethodId = GetMethodId(method: target),
                        StandardElementType = isDependencyBoundary
                            ? StandardElementType.Dependency
                            : localType is null
                                ? StandardElementType.Unknown
                                : Classify(type: localType),
                        IsDependencyBoundary = isDependencyBoundary,
                        TargetSymbol = target,
                        IsExceptionWrapper = target.Name == "TryCatch"
                            && call.Node is InvocationExpressionSyntax invocation
                            && invocation.ArgumentList.Arguments.Any(argument =>
                                argument.Expression is LambdaExpressionSyntax lambda
                                && lambda.DescendantNodes()
                                    .OfType<InvocationExpressionSyntax>()
                                    .Any()),
                    };
                })
            .OrderBy(call => call.MethodId, StringComparer.Ordinal)
            .ToList();
    }

    private static IMethodSymbol? GetCalledMethod(
        CSharpCompilation compilation,
        SyntaxNode call)
    {
        SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree: call.SyntaxTree);
        SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(node: call);

        IMethodSymbol? method = symbolInfo.Symbol as IMethodSymbol
            ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

        return method?.ReducedFrom ?? method;
    }

    private static List<string> GetDirectlyThrownExceptionTypes(
        IMethodSymbol method,
        CSharpCompilation compilation)
    {
        return method
            .DeclaringSyntaxReferences.Select(reference => reference.GetSyntax())
            .SelectMany(node =>
                node.DescendantNodes()
                    .Select(descendant => descendant switch
                    {
                        ThrowStatementSyntax statement => statement.Expression,
                        ThrowExpressionSyntax expression => expression.Expression,
                        _ => null,
                    }))
            .Where(expression => expression is not null)
            .Select(
                expression =>
                    compilation.GetSemanticModel(syntaxTree: expression!.SyntaxTree).GetTypeInfo(expression).Type)
            .Where(type => type is not null)
            .Select(type => GetTypeName(type: type!))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(typeName => typeName, StringComparer.Ordinal)
            .ToList();
    }

    private static List<ExceptionCatch> GetExceptionCatches(
        IMethodSymbol method,
        CSharpCompilation compilation)
    {
        return method
            .DeclaringSyntaxReferences.Select(reference => reference.GetSyntax())
            .SelectMany(node => node.DescendantNodes().OfType<CatchClauseSyntax>())
            .Select(
                catchClause =>
                {
                    SemanticModel semanticModel = compilation.GetSemanticModel(catchClause.SyntaxTree);

                    ITypeSymbol? caughtType = catchClause.Declaration is null
                        ? compilation.GetTypeByMetadataName(fullyQualifiedMetadataName: "System.Exception")
                        : semanticModel.GetTypeInfo(catchClause.Declaration.Type).Type;

                    List<string> thrownTypes = catchClause.Block
                        .DescendantNodes()
                        .OfType<ThrowStatementSyntax>()
                        .Where(statement => statement.Expression is not null)
                        .Select(statement => semanticModel.GetTypeInfo(statement.Expression!).Type)
                        .Where(type => type is not null)
                        .Select(type => GetTypeName(type: type!))
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(typeName => typeName, StringComparer.Ordinal)
                        .ToList();

                    return new ExceptionCatch
                    {
                        ExceptionType = caughtType is null ? string.Empty : GetTypeName(type: caughtType),
                        ThrownExceptionTypes = thrownTypes,
                        Rethrows = catchClause.Block
                            .DescendantNodes()
                            .OfType<ThrowStatementSyntax>()
                            .Any(statement => statement.Expression is null),
                    };
                })
            .ToList();
    }

    private static List<string> GetHttpMethods(IMethodSymbol method)
    {
        IEnumerable<string> symbolAttributeNames = method.GetAttributes()
            .Select(attribute => attribute.AttributeClass?.Name ?? string.Empty)
            .Concat(method.DeclaringSyntaxReferences
                .Select(reference => reference.GetSyntax())
                .OfType<MethodDeclarationSyntax>()
                .SelectMany(declaration => declaration.AttributeLists)
                .SelectMany(attributeList => attributeList.Attributes)
                .Select(attribute => attribute.Name.ToString().Split('.').Last())
                .Select(attributeName => attributeName.EndsWith("Attribute", StringComparison.Ordinal)
                    ? attributeName
                    : $"{attributeName}Attribute"));

        List<string> methods = symbolAttributeNames
            .Select(
                attributeName => attributeName switch
                {
                    "HttpGetAttribute" => "GET",
                    "HttpPostAttribute" => "POST",
                    "HttpPutAttribute" => "PUT",
                    "HttpPatchAttribute" => "PATCH",
                    "HttpDeleteAttribute" => "DELETE",
                    "HttpHeadAttribute" => "HEAD",
                    _ => string.Empty,
                })
            .Where(httpMethod => httpMethod.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(httpMethod => httpMethod, StringComparer.Ordinal)
            .ToList();

        if (methods.Count == 0 && InheritsFromTypeNamed(method.ContainingType, "ODataController"))
        {
            string conventionalMethod = method.Name.EndsWith("Async", StringComparison.Ordinal)
                ? method.Name.Substring(0, method.Name.Length - "Async".Length)
                : method.Name;

            string? httpMethod = conventionalMethod switch
            {
                "Get" or "GetAll" => "GET",
                "Post" => "POST",
                "Put" => "PUT",
                "Patch" => "PATCH",
                "Delete" => "DELETE",
                _ => null,
            };

            if (httpMethod is not null)
            {
                methods.Add(httpMethod);
            }
        }

        return methods;
    }

    private static List<HttpResponse> GetHttpResponses(
        IMethodSymbol method,
        CSharpCompilation compilation)
    {
        List<HttpResponse> responses = [];

        foreach (SyntaxNode declaration in method.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax()))
        {
            IEnumerable<ExpressionSyntax> responseExpressions = declaration
                .DescendantNodes()
                .OfType<ReturnStatementSyntax>()
                .Where(statement => BelongsToMethod(statement: statement, declaration: declaration))
                .Select(statement => statement.Expression)
                .Where(expression => expression is not null)
                .Select(expression => expression!);

            if (declaration is MethodDeclarationSyntax { ExpressionBody.Expression: ExpressionSyntax expressionBody })
            {
                responseExpressions = responseExpressions.Append(expressionBody);
            }

            foreach (ExpressionSyntax responseExpression in responseExpressions)
            {
                foreach (InvocationExpressionSyntax invocation in responseExpression
                    .DescendantNodesAndSelf()
                    .OfType<InvocationExpressionSyntax>())
                {
                    string resultMethod = GetInvocationName(invocation: invocation);

                    int? statusCode = GetStatusCode(
                        resultMethod: resultMethod,
                        invocation: invocation,
                        compilation: compilation);

                    if (statusCode is null)
                    {
                        continue;
                    }

                    CatchClauseSyntax? exceptionCatch = invocation
                        .Ancestors()
                        .OfType<CatchClauseSyntax>()
                        .FirstOrDefault();

                    IfStatementSyntax? nullBranch = invocation
                        .Ancestors()
                        .OfType<IfStatementSyntax>()
                        .FirstOrDefault(
                            statement => statement.Condition.ToString()
                                .Contains("null", StringComparison.Ordinal));

                    ConditionalExpressionSyntax? nullConditional = invocation
                        .Ancestors()
                        .OfType<ConditionalExpressionSyntax>()
                        .FirstOrDefault(
                            conditional => conditional.Condition.ToString()
                                .Contains("null", StringComparison.Ordinal));

                    responses.Add(
                        new HttpResponse
                        {
                            StatusCode = statusCode.Value,
                            ResultMethod = resultMethod,
                            ExceptionType = GetCaughtExceptionType(
                                catchClause: exceptionCatch,
                                compilation: compilation),
                            IsExceptionPath = exceptionCatch is not null,
                            IsNullPath = statusCode == 404
                                && (nullBranch is not null || nullConditional is not null),
                            HasBody = HasResponseBody(
                                resultMethod: resultMethod,
                                invocation: invocation),
                            ExposesExceptionDetails = ExposesExceptionDetails(
                                invocation: invocation,
                                exceptionCatch: exceptionCatch),
                        });
                }
            }

            foreach (AssignmentExpressionSyntax assignment in declaration
                .DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Where(assignment => BelongsToMethod(
                    node: assignment,
                    declaration: declaration))
                .Where(assignment => assignment.Left.ToString()
                    .EndsWith(".Response.StatusCode", StringComparison.Ordinal)))
            {
                int? statusCode = GetConstantStatusCode(
                    expression: assignment.Right,
                    compilation: compilation);

                if (statusCode is null)
                {
                    continue;
                }

                CatchClauseSyntax? exceptionCatch = assignment
                    .Ancestors()
                    .OfType<CatchClauseSyntax>()
                    .FirstOrDefault();

                responses.Add(
                    new HttpResponse
                    {
                        StatusCode = statusCode.Value,
                        ResultMethod = "Response.StatusCode",
                        ExceptionType = GetCaughtExceptionType(
                            catchClause: exceptionCatch,
                            compilation: compilation),
                        IsExceptionPath = exceptionCatch is not null,
                        HasBody = false,
                    });
            }
        }

        return responses
            .GroupBy(
                response => (
                    response.StatusCode,
                    response.ResultMethod,
                    response.ExceptionType,
                    response.IsExceptionPath,
                    response.IsNullPath,
                    response.HasBody,
                    response.ExposesExceptionDetails))
            .Select(group => group.First())
            .OrderBy(response => response.StatusCode)
            .ThenBy(response => response.ResultMethod, StringComparer.Ordinal)
            .ToList();
    }

    private static bool BelongsToMethod(
        ReturnStatementSyntax statement,
        SyntaxNode declaration) =>

        BelongsToMethod(node: statement, declaration: declaration);

    private static bool BelongsToMethod(
        SyntaxNode node,
        SyntaxNode declaration) =>

        !node.Ancestors()
            .TakeWhile(ancestor => ancestor != declaration)
            .Any(
                ancestor => ancestor
                    is AnonymousFunctionExpressionSyntax
                    or LocalFunctionStatementSyntax
                    or MethodDeclarationSyntax);

    private static string GetInvocationName(InvocationExpressionSyntax invocation) =>

        invocation.Expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
            _ => string.Empty,
        };

    private static int? GetStatusCode(
        string resultMethod,
        InvocationExpressionSyntax invocation,
        CSharpCompilation compilation) =>

        resultMethod switch
        {
            "Ok" or "Updated" => 200,
            "Created" or "CreatedAtAction" or "CreatedAtRoute" => 201,
            "NoContent" => 204,
            "BadRequest" => 400,
            "Unauthorized" or "Challenge" => 401,
            "Forbid" => 403,
            "NotFound" => 404,
            "Conflict" => 409,
            "PreconditionFailed" => 412,
            "UnsupportedMediaType" => 415,
            "UnprocessableEntity" => 422,
            "StatusCode" => GetConstantStatusCode(
                expression: invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression,
                compilation: compilation),
            _ => null,
        };

    private static int? GetConstantStatusCode(
        ExpressionSyntax? expression,
        CSharpCompilation compilation)
    {
        if (expression is null)
        {
            return null;
        }

        Optional<object?> constant = compilation
            .GetSemanticModel(expression.SyntaxTree)
            .GetConstantValue(expression);

        return constant.HasValue && constant.Value is int statusCode ? statusCode : null;
    }

    private static bool HasResponseBody(
        string resultMethod,
        InvocationExpressionSyntax invocation) =>

        resultMethod switch
        {
            "Challenge" or "Forbid" or "NoContent" => false,
            "StatusCode" => invocation.ArgumentList.Arguments.Count > 1,
            _ => invocation.ArgumentList.Arguments.Count > 0,
        };

    private static string GetCaughtExceptionType(
        CatchClauseSyntax? catchClause,
        CSharpCompilation compilation)
    {
        if (catchClause is null)
        {
            return string.Empty;
        }

        if (catchClause.Declaration is null)
        {
            return "System.Exception";
        }

        ITypeSymbol? exceptionType = compilation
            .GetSemanticModel(catchClause.SyntaxTree)
            .GetTypeInfo(catchClause.Declaration.Type)
            .Type;

        return exceptionType is null ? string.Empty : GetTypeName(type: exceptionType);
    }

    private static bool ExposesExceptionDetails(
        InvocationExpressionSyntax invocation,
        CatchClauseSyntax? exceptionCatch)
    {
        string exceptionIdentifier = exceptionCatch?.Declaration?.Identifier.Text ?? string.Empty;

        return exceptionIdentifier.Length > 0
            && invocation.ArgumentList.Arguments.Any(
                argument => argument.Expression
                    .DescendantNodesAndSelf()
                    .OfType<IdentifierNameSyntax>()
                    .Any(identifier => identifier.Identifier.Text == exceptionIdentifier));
    }

    private static bool InheritsFromTypeNamed(
        INamedTypeSymbol type,
        string typeName)
    {
        for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
        {
            if (string.Equals(current.Name, typeName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsDeclaredInCurrentProject(
        IMethodSymbol method,
        CSharpCompilation compilation) =>

        SymbolEqualityComparer.Default.Equals(x: method.ContainingAssembly, y: compilation.Assembly)
        && method.Locations.Any(location => location.IsInSource);

    private static List<string> GetImplementedMethodIds(IMethodSymbol method)
    {
        return method.ContainingType.AllInterfaces
            .SelectMany(contract => contract.GetMembers().OfType<IMethodSymbol>())
            .Where(
                contractMethod =>
                    SymbolEqualityComparer.Default.Equals(
                        method.ContainingType.FindImplementationForInterfaceMember(contractMethod),
                        method))
            .Select(GetMethodId)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(methodId => methodId, StringComparer.Ordinal)
            .ToList();
    }

    private static string GetMethodId(IMethodSymbol method) =>

        $"{GetTypeName(type: method.ContainingType)}.{method.Name}({string.Join(",", method.Parameters.Select(parameter => GetTypeName(type: parameter.Type)))})";

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

        if (DeclaresDependencyIntent(type: type)
            && (InheritsFromExternalType(type: type)
                || ImplementsExternalInterface(type: type)
                || HasExternalStateDependency(type: type)))
        {
            return StandardElementType.Dependency;
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

    private static bool HasExternalStateDependency(INamedTypeSymbol type) =>

        type.GetMembers()
            .OfType<IFieldSymbol>()
            .Any(
                predicate: (IFieldSymbol field) =>
                    field.Type.SpecialType == SpecialType.None
                    && !field.Type.Locations.Any(
                        predicate: (Location location) => location.IsInSource));

    private static bool DeclaresDependencyIntent(INamedTypeSymbol type) =>

        type.ContainingNamespace.ToDisplayString()
            .Contains(value: ".Dependencies", comparisonType: StringComparison.Ordinal)
        || type.Name.EndsWith(value: "Dependency", comparisonType: StringComparison.Ordinal);

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
