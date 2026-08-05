// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXRulesProcessingService : ISTXRulesProcessingService
{
    private static readonly IArchitectureModelQueriesProcessingService
        architectureModelQueries = new ArchitectureModelQueriesProcessingService();
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        return EvaluateSTX0024(context: context)
            .Concat(second: ImplementsInfrastructureService(context: context)
                ? []
                : EvaluateSTX0001(context: context)
                    .Concat(second: EvaluateStandardElementTypeRules(context: context)));
    }

    private static IEnumerable<AnalysisItem> EvaluateStandardElementTypeRules(
        EvaluationContext context) =>
        architectureModelQueries.GetStandardElementType(context: context) switch
        {
            StandardElementType.FoundationService
                or StandardElementType.ProcessingService
                or StandardElementType.OrchestrationService
                or StandardElementType.CoordinationService
                or StandardElementType.ManagementService
                or StandardElementType.AggregationService =>
                EvaluateSTX0002(context: context)
                    .Concat(second: EvaluateSTX0003(context: context))
                    .Concat(second: EvaluateSTX0004(context: context))
                    .Concat(second: EvaluateSTX0005(context: context))
                    .Concat(second: EvaluateSTX0006(context: context))
                    .Concat(second: EvaluateSTX0007(context: context))
                    .Concat(second: EvaluateSTX0008(context: context))
                    .Concat(second: EvaluateSTX0009(context: context))
                    .Concat(second: EvaluateSTX0010(context: context))
                    .Concat(second: EvaluateSTX0011(context: context))
                    .Concat(second: EvaluateSTX0012(context: context))
                    .Concat(second: EvaluateSTX0023(context: context))
                    .Concat(second: EvaluateSTX0013(context: context))
                    .Concat(second: EvaluateSTX0014(context: context))
                    .Concat(second: EvaluateSTX0015(context: context))
                    .Concat(second: EvaluateSTX0016(context: context))
                    .Concat(second: EvaluateSTX0017(context: context))
                    .Concat(second: EvaluateSTX0018(context: context))
                    .Concat(second: EvaluateSTX0022(context: context))
                    .Concat(second: EvaluateSTX0019(context: context))
                    .Concat(second: EvaluateSTX0020(context: context))
                    .Concat(second: EvaluateSTX0021(context: context)),
            StandardElementType.Broker =>
                EvaluateSTX0002(context: context)
                    .Concat(second: EvaluateSTX0006(context: context))
                    .Concat(second: EvaluateSTX0017(context: context))
                    .Concat(second: EvaluateSTX0019(context: context))
                    .Concat(second: EvaluateSTX0020(context: context))
                    .Concat(second: EvaluateSTX0021(context: context)),
            StandardElementType.Exposure or StandardElementType.HttpExposure =>
                EvaluateSTX0002(context: context)
                    .Concat(second: EvaluateSTX0017(context: context))
                    .Concat(second: EvaluateSTX0022(context: context))
                    .Concat(second: EvaluateSTX0019(context: context))
                    .Concat(second: EvaluateSTX0020(context: context))
                    .Concat(second: EvaluateSTX0021(context: context)),
            _ => [],
        };

    private static IEnumerable<AnalysisItem> EvaluateSTX0001(
        EvaluationContext context) =>
        architectureModelQueries.GetStandardElementType(context: context) == StandardElementType.Unknown
        && !architectureModelQueries.DeclaresDependencyIntent(context: context)
            ?
            [
                CreateAnalysisItem(
                    code: "STX0001",
                    description: "The type is not a valid Standard element type.",
                    context: context)
            ]
            : [];

    private static IEnumerable<AnalysisItem> EvaluateSTX0024(
        EvaluationContext context)
    {
        InvocationExpressionSyntax[] invocations = architectureModelQueries
            .GetDeclarations(context: context)
            .SelectMany(
                selector: declaration =>
                    declaration.DescendantNodes())
            .OfType<InvocationExpressionSyntax>()
            .ToArray();

        bool allowsCredentials = invocations.Any(
            predicate: invocation =>
                GetInvokedMethodName(invocation: invocation)
                    == "AllowCredentials");

        bool allowsEveryOrigin = invocations.Any(
            predicate: invocation =>
                GetInvokedMethodName(invocation: invocation)
                    == "AllowAnyOrigin"
                || IsAlwaysAllowedOriginInvocation(
                    invocation: invocation));

        return allowsCredentials && allowsEveryOrigin
            ?
            [
                CreateAnalysisItem(
                    code: "STX0024",
                    description:
                        "Credentialed CORS must not allow every origin.",
                    context: context)
            ]
            : [];
    }

    private static string GetInvokedMethodName(
        InvocationExpressionSyntax invocation) =>
        invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess =>
                memberAccess.Name.Identifier.Text,
            IdentifierNameSyntax identifier =>
                identifier.Identifier.Text,
            _ => string.Empty,
        };

    private static bool IsAlwaysAllowedOriginInvocation(
        InvocationExpressionSyntax invocation)
    {
        if (GetInvokedMethodName(invocation: invocation)
            != "SetIsOriginAllowed")
        {
            return false;
        }

        return invocation.ArgumentList.Arguments.Any(
            predicate: argument =>
                argument.Expression switch
                {
                    SimpleLambdaExpressionSyntax simpleLambda =>
                        ReturnsTrue(expression: simpleLambda.Body),
                    ParenthesizedLambdaExpressionSyntax
                        parenthesizedLambda =>
                        ReturnsTrue(
                            expression:
                                parenthesizedLambda.Body),
                    _ => false,
                });
    }

    private static bool ReturnsTrue(CSharpSyntaxNode expression) =>
        expression switch
        {
            LiteralExpressionSyntax literal =>
                literal.IsKind(
                    kind:
                        SyntaxKind.TrueLiteralExpression),
            BlockSyntax block => block.Statements
                .OfType<ReturnStatementSyntax>()
                .Any(predicate: statement =>
                    statement.Expression?.IsKind(
                        kind:
                            SyntaxKind.TrueLiteralExpression)
                    == true),
            _ => false,
        };

    private static bool ImplementsInfrastructureService(EvaluationContext context) =>

        architectureModelQueries.GetImplementedInterfaces(context: context)?.Any(
            predicate: (string interfaceName) =>
                interfaceName.EndsWith(value: ".IRuleProcessingService", comparisonType: StringComparison.Ordinal)
                || interfaceName.EndsWith(
                    value: ".ICodeAnalysisInfrastructureService",
                    comparisonType: StringComparison.Ordinal
                )
        ) == true;

    private static IEnumerable<AnalysisItem> EvaluateSTX0002(EvaluationContext context) =>
        IsEventProviderContract(context: context)
            ? []
            : architectureModelQueries
            .GetDeclarations(context: context).SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<PropertyDeclarationSyntax>()
            .Select(
                selector: (PropertyDeclarationSyntax property) =>
                    CreateAnalysisItem(
                        code: "STX0002",
                        description: "Properties may only be declared on models.",
                        context: context,
                        location: property.GetLocation()
                    )
            );

    private static bool IsEventProviderContract(EvaluationContext context)
    {
        string typeName = architectureModelQueries.GetTypeName(context: context).Split(separator: ['.']).Last();

        return typeName is "EventProvider" or "BulkEventProvider"
            || typeName.StartsWith(value: "EventProvider<", comparisonType: StringComparison.Ordinal)
            || typeName.StartsWith(value: "BulkEventProvider<", comparisonType: StringComparison.Ordinal);
    }

    private static IEnumerable<AnalysisItem> EvaluateSTX0003(EvaluationContext context)
    {
        MethodDeclarationSyntax[] methods = architectureModelQueries
            .GetDeclarations(context: context).Where(
                predicate: (TypeDeclarationSyntax declaration) =>
                    !declaration.SyntaxTree.FilePath.EndsWith(
                        value: ".Validations.cs",
                        comparisonType: StringComparison.Ordinal
                    )
                    && !declaration.SyntaxTree.FilePath.EndsWith(
                        value: ".Exceptions.cs",
                        comparisonType: StringComparison.Ordinal
                    )
            )
            .SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .ToArray();

        return (methods.Length == 0 || !methods.All(predicate: IsSinglePassThroughMethod))
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: "STX0003",
                    description: "A service containing only pass-through methods is redundant and should be retired.",
                    context: context
                ),
            };
    }

    private static AnalysisItem[] CreateDependencyLayerAnalysisItems(
        EvaluationContext context,
        StandardElementType expectedDependencyType,
        string code
    )
    {
        int count = architectureModelQueries.GetDependencies(context: context).Count;
        bool flag = (uint)(count - 2) <= 1u;
        bool hasValidCount = flag;

        bool containsOnlyExpectedDependencies = architectureModelQueries.GetDependencies(context: context).All(
            predicate: (TypeDependency dependency) => dependency.StandardElementType == expectedDependencyType
        );

        return (hasValidCount && containsOnlyExpectedDependencies)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: code,
                    description: $"The service must have two or three {expectedDependencyType} dependencies.",
                    context: context
                ),
            };
    }

    private static IEnumerable<AnalysisItem> EvaluateSTX0004(EvaluationContext context)
    {
        return (
            !architectureModelQueries.GetDependencies(context: context).Any(
                predicate: (TypeDependency dependency) => dependency.StandardElementType == architectureModelQueries.GetStandardElementType(context: context)
            )
        )
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: "STX0004",
                    description: "A service must not depend on another service at the same layer.",
                    context: context
                ),
            };
    }

    private static IEnumerable<AnalysisItem> EvaluateSTX0005(EvaluationContext context) =>

        architectureModelQueries.GetPublicMethodCallLineNumbers(context: context).Select(
            selector: (int lineNumber) =>
                new AnalysisItem
                {
                    Code = "STX0005",
                    Description = "A public service method must not call another public method on the same service.",
                    Severity = AnalysisSeverity.Warning,
                    Type = architectureModelQueries.GetTypeName(context: context),
                    LineNumber = lineNumber,
                }
        );

    private static IEnumerable<AnalysisItem> EvaluateSTX0006(EvaluationContext context)
    {
        return (!architectureModelQueries.IsPublic(context: context))
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: "STX0006",
                    description: "Business implementation classes should be internal by default.",
                    context: context
                ),
            };
    }

    private static IEnumerable<AnalysisItem> EvaluateSTX0007(EvaluationContext context)
    {
        return (architectureModelQueries.GetPublicApiModelTypes(context: context).Count <= 1)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1]
            {
                CreateAnalysisItem(
                    code: "STX0007",
                    description: "A service public API must use one model contract or primitive types.",
                    context: context
                ),
            };
    }

    private static IEnumerable<AnalysisItem> EvaluateSTX0008(EvaluationContext context) =>
        CreateWhenInvalid(
            isInvalid: !HasPartial(context: context, suffix: ".Validations.cs"),
            code: "STX0008",
            description:
                $"A service must declare its validations in {GetPartialFileName(context: context, suffix: ".Validations.cs")}.",
            context: context);

    private static IEnumerable<AnalysisItem> EvaluateSTX0009(EvaluationContext context) =>
        CreateWhenInvalid(
            isInvalid: !HasPartial(context: context, suffix: ".Exceptions.cs"),
            code: "STX0009",
            description:
                $"A service must declare TryCatch handling in {GetPartialFileName(context: context, suffix: ".Exceptions.cs")}.",
            context: context);

    private static IEnumerable<AnalysisItem> EvaluateSTX0010(EvaluationContext context) =>
        CreateWhenInvalid(
            isInvalid: !GetPublicMethods(context: context).All(predicate: UsesTryCatch),
            code: "STX0010",
            description: "Every public service method must enter through a local TryCatch operation.",
            context: context);

    private static IEnumerable<AnalysisItem> EvaluateSTX0011(EvaluationContext context) =>
        CreateWhenInvalid(
            isInvalid: !GetPublicMethods(context: context).All(predicate: ValidatesInputs),
            code: "STX0011",
            description: "Every service input must be validated inside TryCatch before business work.",
            context: context);

    private static IEnumerable<AnalysisItem> EvaluateSTX0012(EvaluationContext context)
    {
        if (architectureModelQueries.GetStandardElementType(context: context) != StandardElementType.FoundationService)
        {
            return [];
        }

        MethodDeclarationSyntax[] publicMethods = GetPublicMethods(context: context);

        if (!publicMethods.Any(predicate: RequiresOperationSpecificValidation))
        {
            return [];
        }

        MethodDeclarationSyntax[] operationValidationMethods = architectureModelQueries
            .GetDeclarations(context: context)
            .Where(predicate: declaration =>
                declaration.SyntaxTree.FilePath.EndsWith(
                    value: ".Validations.cs",
                    comparisonType: StringComparison.Ordinal))
            .SelectMany(selector: declaration => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .Where(predicate: method =>
                method.Identifier.Text.StartsWith(value: "Validate", comparisonType: StringComparison.Ordinal)
                && method.Identifier.Text.Contains(value: "On", comparisonType: StringComparison.Ordinal))
            .ToArray();

        bool usesValidationCollector = operationValidationMethods.Length != 0
            && operationValidationMethods.All(predicate: method =>
                method.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Any(predicate: invocation =>
                        invocation.Expression.ToString().EndsWith(
                            value: "Validate",
                            comparisonType: StringComparison.Ordinal)));

        return CreateWhenInvalid(
            isInvalid: !usesValidationCollector,
            code: "STX0012",
            description: "Business-operation validation methods must evaluate their rules through a validation collector.",
            context: context);
    }

    private static IEnumerable<AnalysisItem> EvaluateSTX0013(EvaluationContext context) =>
        CreateWhenInvalid(
            isInvalid: architectureModelQueries.GetImplementedInterfaces(context: context).Count == 0,
            code: "STX0013",
            description: "A service must implement a local interface.",
            context: context);

    private static IEnumerable<AnalysisItem> EvaluateSTX0014(EvaluationContext context) =>
        CreateWhenInvalid(
            isInvalid: !ImplementsMatchingInterface(context: context),
            code: "STX0014",
            description: "A service contract must be named after its implementation with an I prefix.",
            context: context);

    private static IEnumerable<AnalysisItem> EvaluateSTX0015(EvaluationContext context) =>
        CreateWhenInvalid(
            isInvalid: !ContractContainsPublicMethods(context: context),
            code: "STX0015",
            description: "Every public service method must be declared by its local interface.",
            context: context);

    private static IEnumerable<AnalysisItem> EvaluateSTX0023(EvaluationContext context) =>
        CreateWhenInvalid(
            isInvalid: architectureModelQueries.GetStandardElementType(context: context) == StandardElementType.FoundationService
                && !GetPublicMethods(context: context).All(predicate: UsesOperationSpecificValidation),
            code: "STX0023",
            description: "Each business operation must call its operation-specific validation method.",
            context: context);

    private static bool HasPartial(EvaluationContext context, string suffix) =>
        architectureModelQueries.GetDeclarations(context: context).Any(predicate: declaration =>
            string.Equals(
                a: Path.GetFileName(declaration.SyntaxTree.FilePath),
                b: GetPartialFileName(context: context, suffix: suffix),
                comparisonType: StringComparison.Ordinal));

    private static string GetPartialFileName(EvaluationContext context, string suffix) =>
        $"{architectureModelQueries.GetTypeName(context: context).Split(separator: ['.']).Last()}{suffix}";

    private static MethodDeclarationSyntax[] GetPublicMethods(EvaluationContext context) =>
        architectureModelQueries.GetDeclarations(context: context)
            .SelectMany(selector: declaration => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .Where(predicate: method => method.Modifiers.Any(kind: SyntaxKind.PublicKeyword))
            .ToArray();

    private static bool HasWrappedExceptionCategory(MethodDeclarationSyntax method, string categoryName) =>

        method
            .DescendantNodes()
            .OfType<CatchClauseSyntax>()
            .Any(
                predicate: (CatchClauseSyntax catchClause) =>
                    CatchWrapsException(catchClause: catchClause, categoryName: categoryName)
            );

    private static bool HasWrappedDefaultException(MethodDeclarationSyntax method) =>

        method
            .DescendantNodes()
            .OfType<CatchClauseSyntax>()
            .Any(
                predicate: (CatchClauseSyntax catchClause) =>
                    catchClause.Declaration?.Type.ToString() == "Exception"
                    && CatchWrapsException(catchClause: catchClause, categoryName: null)
            );

    private static bool CatchWrapsException(CatchClauseSyntax catchClause, string? categoryName)
    {
        string caughtExceptionName = catchClause.Declaration?.Identifier.Text ?? string.Empty;

        return caughtExceptionName.Length > 0
            && catchClause
                .Block.DescendantNodes()
            .OfType<ThrowStatementSyntax>()
                .Select(selector: (ThrowStatementSyntax throwStatement) => throwStatement.Expression)
                .OfType<ObjectCreationExpressionSyntax>()
                .Any(
                    predicate: (ObjectCreationExpressionSyntax objectCreation) =>
                        IsExpectedExceptionCategory(objectCreation: objectCreation, categoryName: categoryName)
                        && objectCreation.ArgumentList?.Arguments.Any(
                            predicate: (ArgumentSyntax argument) =>
                                argument
                                    .Expression.DescendantNodesAndSelf()
            .OfType<IdentifierNameSyntax>()
                                    .Any(
                                        predicate: (IdentifierNameSyntax identifier) =>
                                            identifier.Identifier.Text == caughtExceptionName
                                    )
                        ) == true
                );
    }

    private static bool IsExpectedExceptionCategory(ObjectCreationExpressionSyntax objectCreation, string? categoryName)
    {
        string exceptionType = objectCreation.Type.ToString();

        return categoryName is not null
            ? exceptionType.Contains(value: categoryName, comparisonType: StringComparison.Ordinal)
            : !exceptionType.Contains(value: "Validation", comparisonType: StringComparison.Ordinal)
                && !exceptionType.Contains(value: "Dependency", comparisonType: StringComparison.Ordinal);
    }

    private static IEnumerable<AnalysisItem> EvaluateSTX0016(EvaluationContext context)
    {
        string[] nonDomainVerbs = new string[4] { "Select", "Insert", "Post", "Put" };

        return architectureModelQueries
            .GetDeclarations(context: context).SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .Where(
                predicate: (MethodDeclarationSyntax method) =>
                    method.Modifiers.Any(kind: SyntaxKind.PublicKeyword)
                    && nonDomainVerbs.Any(
                        predicate: (string verb) =>
                            method.Identifier.Text.StartsWith(value: verb, comparisonType: StringComparison.Ordinal)
                    )
            )
            .Select(
                selector: (MethodDeclarationSyntax method) =>
                    CreateAnalysisItem(
                        code: "STX0016",
                        description: "Service CRUD methods must use domain nouns: Get, Add, Update, or Delete.",
                        context: context,
                        location: method.GetLocation()
                    )
            );
    }

    private static IEnumerable<AnalysisItem> EvaluateSTX0017(EvaluationContext context) =>

        architectureModelQueries
            .GetDeclarations(context: context).SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .SelectMany(selector: (MethodDeclarationSyntax method) => method.ParameterList.Parameters)
            .Where(predicate: (ParameterSyntax parameter) => parameter.Identifier.Text == "id")
            .Select(
                selector: (ParameterSyntax parameter) =>
                    CreateAnalysisItem(
                        code: "STX0017",
                        description: "Identifier parameters must be named for their type, for example studentId.",
                        context: context,
                        location: parameter.GetLocation()
                    )
            );

    private static IEnumerable<AnalysisItem> EvaluateSTX0019(EvaluationContext context) =>
        EvaluateMutationNaming(
            context: context,
            operation: "create",
            expectedPrefix: "new",
            code: "STX0019");

    private static IEnumerable<AnalysisItem> EvaluateSTX0020(EvaluationContext context) =>
        EvaluateMutationNaming(
            context: context,
            operation: "update",
            expectedPrefix: "updated",
            code: "STX0020");

    private static IEnumerable<AnalysisItem> EvaluateSTX0021(EvaluationContext context) =>
        EvaluateMutationNaming(
            context: context,
            operation: "delete",
            expectedPrefix: "deleted",
            code: "STX0021");

    private static IEnumerable<AnalysisItem> EvaluateMutationNaming(
        EvaluationContext context,
        string operation,
        string expectedPrefix,
        string code)
    {
        if (architectureModelQueries.GetTypeName(context: context).EndsWith(value: ".IServiceCollectionExtensions", comparisonType: StringComparison.Ordinal))
        {
            return Array.Empty<AnalysisItem>();
        }

        List<AnalysisItem> items = new List<AnalysisItem>();

        foreach (
            MethodDeclarationSyntax method in architectureModelQueries
                .GetDeclarations(context: context).SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
        )
        {
            string methodName = method.Identifier.Text;
            string? methodOperation = GetMutationOperation(methodName: methodName);

            if (methodOperation != operation
                || operation == "create"
                    && methodName.StartsWith(value: "AddOrUpdate", comparisonType: StringComparison.Ordinal))
            {
                continue;
            }

            (ParameterSyntax Parameter, string? TypeName) modelParameter = method
                .ParameterList.Parameters.Select(
                    selector: (ParameterSyntax parameter) =>
                        (Parameter: parameter, TypeName: GetModelTypeName(parameter: parameter, context: context))
                )
                .FirstOrDefault(predicate: item => item.TypeName is not null);

            string? modelTypeName = modelParameter.TypeName;

            if (modelTypeName == null)
            {
                continue;
            }

            if (!modelParameter.Item1.Identifier.Text.StartsWith(
                    value: expectedPrefix,
                    comparisonType: StringComparison.Ordinal
                )
            )
            {
                items.Add(
                    item: CreateAnalysisItem(
                        code: code,
                        description: operation + " model parameters must use the " + expectedPrefix + " prefix.",
                        context: context,
                        location: modelParameter.Item1.GetLocation()
                    )
                );
            }
        }

        return items;
    }

    private static IEnumerable<AnalysisItem> EvaluateSTX0018(EvaluationContext context) =>

        architectureModelQueries
            .GetDeclarations(context: context).SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .Where(predicate: (MethodDeclarationSyntax method) => method.Modifiers.Any(kind: SyntaxKind.PublicKeyword))
            .Select(
                selector: (MethodDeclarationSyntax method) =>
                    new
                    {
                        Method = method,
                        ModelTypes = method
                            .ParameterList.Parameters.Select(
                                selector: (ParameterSyntax parameter) =>
                                    GetModelTypeName(parameter: parameter, context: context)
                            )
            .OfType<string>()
                            .Distinct(comparer: StringComparer.Ordinal)
                            .ToArray(),
                    }
            )
            .Where(predicate: item =>
                item.ModelTypes.Any(
                    predicate: (string typeName) =>
                        !item.Method.Identifier.Text.Contains(value: typeName, comparisonType: StringComparison.Ordinal)
                )
            )
            .Select(selector: item =>
                CreateAnalysisItem(
                    code: "STX0018",
                    description: "Service method names must include each model type they operate on.",
                    context: context,
                    location: item.Method.GetLocation()
                )
            );

    private static IEnumerable<AnalysisItem> EvaluateSTX0022(EvaluationContext context) =>

        architectureModelQueries
            .GetDeclarations(context: context).SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .Where(
                predicate: (MethodDeclarationSyntax method) =>
                    method.Identifier.Text.StartsWith(value: "Create", comparisonType: StringComparison.Ordinal)
            )
            .Select(
                selector: (MethodDeclarationSyntax method) =>
                    new
                    {
                        Method = method,
                        ReturnType = method
                            .ReturnType.DescendantNodesAndSelf()
            .OfType<SimpleNameSyntax>()
                            .LastOrDefault()
                            ?.Identifier.Text,
                    }
            )
            .Where(predicate: item =>
            {
                if (item.ReturnType is null)
                {
                    return false;
                }

                string contractName =
                    item.ReturnType.Length > 1
                    && item.ReturnType[0] == 'I'
                    && char.IsUpper(c: item.ReturnType[1])
                        ? item.ReturnType.Substring(
                            startIndex: 1)
                        : item.ReturnType;

                return !item.Method.Identifier.Text.Contains(
                    value: contractName,
                    comparisonType: StringComparison.Ordinal);
            })
            .Select(selector: item =>
                CreateAnalysisItem(
                    code: "STX0022",
                    description: "Creation method names must include the concrete type they create.",
                    context: context,
                    location: item.Method.GetLocation()
                )
            );

    private static string? GetMutationOperation(string methodName)
    {
        if (
            methodName.StartsWith(value: "Add", comparisonType: StringComparison.Ordinal)
            || methodName.StartsWith(value: "Insert", comparisonType: StringComparison.Ordinal)
            || methodName.StartsWith(value: "Post", comparisonType: StringComparison.Ordinal)
        )
        {
            return "create";
        }

        if (
            methodName.StartsWith(value: "Update", comparisonType: StringComparison.Ordinal)
            || methodName.StartsWith(value: "Put", comparisonType: StringComparison.Ordinal)
        )
        {
            return "update";
        }

        return methodName.StartsWith(value: "Delete", comparisonType: StringComparison.Ordinal) ? "delete" : null;
    }

    private static string? GetModelTypeName(ParameterSyntax parameter, EvaluationContext context)
    {
        string[] candidateNames =
            parameter
                .Type?.DescendantNodesAndSelf()
            .OfType<SimpleNameSyntax>()
                .Select(selector: (SimpleNameSyntax name) => name.Identifier.Text)
                .Reverse()
                .ToArray()
            ?? [];

        return candidateNames.FirstOrDefault(
            predicate: (string candidate) =>
                architectureModelQueries.GetPublicApiModelTypes(context: context).Any(
                    predicate: (string modelType) =>
                        modelType.EndsWith(value: "." + candidate, comparisonType: StringComparison.Ordinal)
                )
        );
    }

    private static bool ImplementsMatchingInterface(EvaluationContext context)
    {
        if (architectureModelQueries.GetImplementedInterfaces(context: context).Count == 0)
        {
            return true;
        }

        string typeName = architectureModelQueries.GetTypeName(context: context).Split(separator: ['.'])
            .Last();

        string expectedInterfaceName = "I" + typeName;

        return architectureModelQueries.GetImplementedInterfaces(context: context).Any(
            predicate: (string interfaceName) => interfaceName.Split(separator: ['.'])
            .Last() == expectedInterfaceName
        );
    }

    private static bool ContractContainsPublicMethods(EvaluationContext context)
    {
        return architectureModelQueries.GetImplementedInterfaces(context: context).Count == 0
            || architectureModelQueries.GetPublicMethodNames(context: context).All(
                predicate: ((IEnumerable<string>)architectureModelQueries.GetContractMethodNames(context: context)).Contains<string>
            );
    }

    private static AnalysisItem[] CreateWhenInvalid(
        bool isInvalid,
        string code,
        string description,
        EvaluationContext context
    )
    {
        return (!isInvalid)
            ? Array.Empty<AnalysisItem>()
            : new AnalysisItem[1] { CreateAnalysisItem(code: code, description: description, context: context) };
    }

    private static bool UsesTryCatch(MethodDeclarationSyntax method) =>

        method
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Any(
                predicate: (InvocationExpressionSyntax invocation) =>
                    IsTryCatchInvocation(expression: invocation.Expression)
                    && invocation.ArgumentList.Arguments.Any(
                        predicate: (ArgumentSyntax argument) => argument.Expression is LambdaExpressionSyntax
                    )
            );

    private static bool IsTryCatchInvocation(ExpressionSyntax expression)
    {
        return expression is IdentifierNameSyntax identifierName && identifierName.Identifier.Text == "TryCatch"
            || expression is GenericNameSyntax genericName && genericName.Identifier.Text == "TryCatch";
    }

    private static bool ValidatesInputs(MethodDeclarationSyntax method)
    {
        string[] parameters = method
            .ParameterList.Parameters.Where(
                predicate: delegate (ParameterSyntax parameter)
                {
                    EqualsValueClauseSyntax? equalsValueClauseSyntax = parameter.Default;

                    return equalsValueClauseSyntax == null
                        || !equalsValueClauseSyntax.Value.IsKind(kind: SyntaxKind.NullLiteralExpression);
                }
            )
            .Select(selector: (ParameterSyntax parameter) => parameter.Identifier.Text)
            .ToArray();

        if (parameters.Length == 0)
        {
            return true;
        }

        InvocationExpressionSyntax? validation = method
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault(
                predicate: (InvocationExpressionSyntax invocation) =>
                    invocation
                        .Expression.ToString()
            .StartsWith(value: "Validate", comparisonType: StringComparison.Ordinal)
            );

        return validation != null
            && parameters.All(
                predicate: (string parameter) =>
                    validation.ArgumentList.Arguments.Any(
                        predicate: (ArgumentSyntax argument) =>
                            argument
                                .Expression.DescendantNodesAndSelf()
            .OfType<IdentifierNameSyntax>()
                                .Any(
                                    predicate: (IdentifierNameSyntax identifier) =>
                                        identifier.Identifier.Text == parameter
                                )
                    )
            );
    }

    private static bool UsesOperationSpecificValidation(MethodDeclarationSyntax method)
    {
        if (!RequiresOperationSpecificValidation(method: method))
        {
            return true;
        }

        string methodName = method.Identifier.Text;

        string operationName = methodName.EndsWith(value: "Async", comparisonType: StringComparison.Ordinal)
            ? methodName.Substring(startIndex: 0, length: methodName.Length - "Async".Length)
            : methodName;

        string[] operations = { "Retrieve", "Update", "Delete", "Add", "Get" };

        string? operation = operations.FirstOrDefault(
            predicate: (string candidate) =>
                operationName.StartsWith(value: candidate, comparisonType: StringComparison.Ordinal)
        );

        string entity = operationName.Substring(startIndex: operation!.Length);
        string expectedValidation = $"Validate{entity}On{operation}";

        return method
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Any(
                predicate: (InvocationExpressionSyntax invocation) =>
                    invocation.Expression.ToString() == expectedValidation
            );
    }

    private static bool RequiresOperationSpecificValidation(MethodDeclarationSyntax method)
    {
        if (method.ParameterList.Parameters.Count == 0)
        {
            return false;
        }

        string methodName = method.Identifier.Text.EndsWith(value: "Async", comparisonType: StringComparison.Ordinal)
            ? method.Identifier.Text.Substring(startIndex: 0, length: method.Identifier.Text.Length - "Async".Length)
            : method.Identifier.Text;

        string[] operations = { "Retrieve", "Update", "Delete", "Add", "Get" };

        return operations.Any(
            predicate: (string operation) =>
                methodName.StartsWith(value: operation, comparisonType: StringComparison.Ordinal)
                && methodName.Length > operation.Length
        );
    }

    private static AnalysisItem CreateAnalysisItem(
        string code,
        string description,
        EvaluationContext context,
        Location? location = null
    )
    {
        return new AnalysisItem
        {
            Code = code,
            Description = description,
            Severity = AnalysisSeverity.Warning,
            Type = architectureModelQueries.GetTypeName(context: context),
            LineNumber = (
                location is not null ? location.GetLineSpan().StartLinePosition.Line + 1 : architectureModelQueries.GetLineNumber(context: context)
            ),
        };
    }

    private static bool IsSinglePassThroughMethod(MethodDeclarationSyntax method)
    {
        LambdaExpressionSyntax? tryCatchLambda =
            method
                .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault(
                    predicate: (InvocationExpressionSyntax invocation) => invocation.Expression.ToString() == "TryCatch"
                )
                ?.ArgumentList.Arguments.FirstOrDefault()
                ?.Expression as LambdaExpressionSyntax;

        if (tryCatchLambda?.Body is ExpressionSyntax lambdaExpression)
        {
            return IsInvocation(expression: lambdaExpression);
        }

        if (tryCatchLambda?.Body is BlockSyntax lambdaBlock)
        {
            StatementSyntax[] businessStatements = lambdaBlock
                .Statements.Where(
                    predicate: (StatementSyntax statement) =>
                        !statement.ToString()
                .StartsWith(value: "Validate(", comparisonType: StringComparison.Ordinal)
                )
                .ToArray();

            return businessStatements.Length == 1 && IsInvocationStatement(statement: businessStatements[0]);
        }

        if (method.ExpressionBody != null)
        {
            return IsInvocation(expression: method.ExpressionBody.Expression);
        }

        BlockSyntax? body2 = method.Body;

        if (body2 == null || body2.Statements.Count != 1)
        {
            return false;
        }

        return IsInvocationStatement(statement: body2.Statements[index: 0]);
    }

    private static bool IsInvocationStatement(StatementSyntax statement) =>

        statement switch
        {
            ReturnStatementSyntax { Expression: not null } returnStatement => IsInvocation(
                expression: returnStatement.Expression
            ),
            ExpressionStatementSyntax expressionStatement => IsInvocation(expression: expressionStatement.Expression),
            _ => false,
        };

    private static bool IsInvocation(ExpressionSyntax expression)
    {
        if (expression is AwaitExpressionSyntax awaitExpression)
        {
            return IsInvocation(expression: awaitExpression.Expression);
        }

        return expression is InvocationExpressionSyntax invocation
            && invocation.ArgumentList.Arguments.All(
                predicate: (ArgumentSyntax argument) => argument.Expression is IdentifierNameSyntax
            );
    }
}
