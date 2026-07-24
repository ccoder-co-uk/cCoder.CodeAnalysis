// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Services.Processings.Rules;

internal sealed class STXRulesProcessingService : ISTXRulesProcessingService
{
    public IEnumerable<AnalysisItem> Evaluate(EvaluationContext context)
    {
        if (ImplementsInfrastructureService(context: context))
        {
            yield break;
        }

        if (
            context.StandardElementType == StandardElementType.Unknown
            && !context.DeclaresDependencyIntent
        )
        {
            yield return CreateAnalysisItem(
                code: "STX0001",
                description: "The type is not a valid Standard element type.",
                context: context
            );
        }
        else if (
            context.StandardElementType
            is StandardElementType.FoundationService
                or StandardElementType.ProcessingService
                or StandardElementType.OrchestrationService
                or StandardElementType.CoordinationService
                or StandardElementType.ManagementService
                or StandardElementType.AggregationService
        )
        {
            foreach (
                AnalysisItem item in EvaluateSTX0002(context: context)
                .Concat(second: EvaluateSTX0003(context: context))
                    .Concat(second: EvaluateSTX0004(context: context))
                    .Concat(second: EvaluateSTX0005(context: context))
                    .Concat(second: EvaluateSTX0006(context: context))
                    .Concat(second: EvaluateSTX0007(context: context))
                    .Concat(
                        second: EvaluateServiceContractPattern(context: context)
                .Where(
                                predicate: (AnalysisItem item) =>
                                    item.Code.StartsWith(value: "STX0", comparisonType: StringComparison.Ordinal)
                            )
                    )
            )
            {
                yield return item;
            }
        }
        else if (context.StandardElementType == StandardElementType.Broker)
        {
            foreach (
                AnalysisItem item in EvaluateSTX0002(context: context)
                .Concat(second: EvaluateSTX0006(context: context))
                    .Concat(second: EvaluateSTX0017(context: context))
                    .Concat(second: EvaluateMutationNaming(context: context))
            )
            {
                yield return item;
            }
        }
        else if (context.StandardElementType == StandardElementType.Exposure)
        {
            foreach (
                AnalysisItem item in EvaluateSTX0002(context: context)
                .Concat(second: EvaluateSTX0017(context: context))
                    .Concat(second: EvaluateSTX0022(context: context))
                    .Concat(second: EvaluateMutationNaming(context: context))
            )
            {
                yield return item;
            }
        }
    }

    private static bool ImplementsInfrastructureService(EvaluationContext context) =>

        context.ImplementedInterfaces?.Any(
            predicate: (string interfaceName) =>
                interfaceName.EndsWith(value: ".IRuleProcessingService", comparisonType: StringComparison.Ordinal)
                || interfaceName.EndsWith(
                    value: ".ICodeAnalysisInfrastructureService",
                    comparisonType: StringComparison.Ordinal
                )
        ) == true;

    private static IEnumerable<AnalysisItem> EvaluateSTX0002(EvaluationContext context) =>

        context
            .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
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

    private static IEnumerable<AnalysisItem> EvaluateSTX0003(EvaluationContext context)
    {
        MethodDeclarationSyntax[] methods = context
            .Declarations.Where(
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
        int count = context.Dependencies.Count;
        bool flag = (uint)(count - 2) <= 1u;
        bool hasValidCount = flag;

        bool containsOnlyExpectedDependencies = context.Dependencies.All(
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
            !context.Dependencies.Any(
                predicate: (TypeDependency dependency) => dependency.StandardElementType == context.StandardElementType
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

        context.PublicMethodCallLineNumbers.Select(
            selector: (int lineNumber) =>
                new AnalysisItem
                {
                    Code = "STX0005",
                    Description = "A public service method must not call another public method on the same service.",
                    Severity = AnalysisSeverity.Warning,
                    Type = context.TypeName,
                    LineNumber = lineNumber,
                }
        );

    private static IEnumerable<AnalysisItem> EvaluateSTX0006(EvaluationContext context)
    {
        return (!context.IsPublic)
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
        return (context.PublicApiModelTypes.Count <= 1)
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

    private static IEnumerable<AnalysisItem> EvaluateServiceContractPattern(EvaluationContext context)
    {
        MethodDeclarationSyntax[] publicMethods = context
            .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .Where(predicate: (MethodDeclarationSyntax method) => method.Modifiers.Any(kind: SyntaxKind.PublicKeyword))
            .ToArray();

        bool hasValidationPartial = context.Declarations.Any(
            predicate: (TypeDeclarationSyntax declaration) =>
                declaration.SyntaxTree.FilePath.EndsWith(
                    value: ".Validations.cs",
                    comparisonType: StringComparison.Ordinal
                )
        );

        bool hasExceptionPartial = context.Declarations.Any(
            predicate: (TypeDeclarationSyntax declaration) =>
                declaration.SyntaxTree.FilePath.EndsWith(
                    value: ".Exceptions.cs",
                    comparisonType: StringComparison.Ordinal
                )
        );

        bool allUseTryCatch = publicMethods.All(predicate: UsesTryCatch);
        bool allValidateInputs = publicMethods.All(predicate: ValidatesInputs);
        bool isFoundationService = context.StandardElementType == StandardElementType.FoundationService;
        bool requiresOperationValidation = publicMethods.Any(predicate: RequiresOperationSpecificValidation);

        bool allUseOperationSpecificValidations =
            !isFoundationService || publicMethods.All(predicate: UsesOperationSpecificValidation);

        MethodDeclarationSyntax[] operationValidationMethods = context
            .Declarations.Where(
                predicate: (TypeDeclarationSyntax declaration) =>
                    declaration.SyntaxTree.FilePath.EndsWith(
                        value: ".Validations.cs",
                        comparisonType: StringComparison.Ordinal
                    )
            )
            .SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
            .Where(
                predicate: (MethodDeclarationSyntax method) =>
                    method.Identifier.Text.StartsWith(value: "Validate", comparisonType: StringComparison.Ordinal)
                    && method.Identifier.Text.Contains(value: "On", comparisonType: StringComparison.Ordinal)
            )
            .ToArray();

        bool usesValidationCollector =
            !isFoundationService
            || !requiresOperationValidation
            || operationValidationMethods.Length != 0
                && operationValidationMethods.All(
                    predicate: (MethodDeclarationSyntax method) =>
                        method
                            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
                            .Any(
                                predicate: (InvocationExpressionSyntax invocation) =>
                                    invocation
                                        .Expression.ToString()
            .EndsWith(value: "Validate", comparisonType: StringComparison.Ordinal)
                            )
                );

        List<AnalysisItem> list = new List<AnalysisItem>();

        list.AddRange(
            collection: CreateWhenInvalid(
                isInvalid: !hasValidationPartial,
                code: "STX0008",
                description: "A service must declare its validations in a Validations partial.",
                context: context
            )
        );

        list.AddRange(
            collection: CreateWhenInvalid(
                isInvalid: !hasExceptionPartial,
                code: "STX0009",
                description: "A service must declare TryCatch handling in an Exceptions partial.",
                context: context
            )
        );

        list.AddRange(
            collection: CreateWhenInvalid(
                isInvalid: !allUseTryCatch,
                code: "STX0010",
                description: "Every public service method must enter through a local TryCatch operation.",
                context: context
            )
        );

        list.AddRange(
            collection: CreateWhenInvalid(
                isInvalid: !allValidateInputs,
                code: "STX0011",
                description: "Every service input must be validated inside TryCatch before business work.",
                context: context
            )
        );

        list.AddRange(
            collection: CreateWhenInvalid(
                isInvalid: !usesValidationCollector,
                code: "STX0012",
                description: "Business-operation validation methods must evaluate their rules through a validation collector.",
                context: context
            )
        );

        list.AddRange(
            collection: CreateWhenInvalid(
                isInvalid: !allUseOperationSpecificValidations,
                code: "STX0023",
                description: "Each business operation must call its operation-specific validation method.",
                context: context
            )
        );

        list.AddRange(
            collection: CreateWhenInvalid(
                isInvalid: context.ImplementedInterfaces.Count == 0,
                code: "STX0013",
                description: "A service must implement a local interface.",
                context: context
            )
        );

        list.AddRange(
            collection: CreateWhenInvalid(
                isInvalid: !ImplementsMatchingInterface(context: context),
                code: "STX0014",
                description: "A service contract must be named after its implementation with an I prefix.",
                context: context
            )
        );

        list.AddRange(
            collection: CreateWhenInvalid(
                isInvalid: !ContractContainsPublicMethods(context: context),
                code: "STX0015",
                description: "Every public service method must be declared by its local interface.",
                context: context
            )
        );

        list.AddRange(collection: EvaluateSTX0016(context: context));
        list.AddRange(collection: EvaluateSTX0017(context: context));
        list.AddRange(collection: EvaluateSTX0018(context: context));
        list.AddRange(collection: EvaluateSTX0022(context: context));
        list.AddRange(collection: EvaluateMutationNaming(context: context));
        return list;
    }

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

        return context
            .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
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

        context
            .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
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

    private static IEnumerable<AnalysisItem> EvaluateMutationNaming(EvaluationContext context)
    {
        if (context.TypeName.EndsWith(value: ".IServiceCollectionExtensions", comparisonType: StringComparison.Ordinal))
        {
            return Array.Empty<AnalysisItem>();
        }

        List<AnalysisItem> items = new List<AnalysisItem>();

        foreach (
            MethodDeclarationSyntax method in context
                .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
            .OfType<MethodDeclarationSyntax>()
        )
        {
            string methodName = method.Identifier.Text;
            string? operation = GetMutationOperation(methodName: methodName);

            if (operation == null)
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

            string? expectedPrefix = operation switch
            {
                "create" when !methodName.StartsWith(value: "AddOrUpdate", comparisonType: StringComparison.Ordinal) =>
                    "new",
                "update" => "updated",
                "delete" => "deleted",
                _ => null,
            };

            if (
                expectedPrefix != null
                && !modelParameter.Item1.Identifier.Text.StartsWith(
                    value: expectedPrefix,
                    comparisonType: StringComparison.Ordinal
                )
            )
            {
                string code = operation switch
                {
                    "create" => "STX0019",
                    "update" => "STX0020",
                    _ => "STX0021",
                };

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

        context
            .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
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

        context
            .Declarations.SelectMany(selector: (TypeDeclarationSyntax declaration) => declaration.Members)
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
                item.ReturnType is not null
                && !item.Method.Identifier.Text.Contains(
                    value: item.ReturnType,
                    comparisonType: StringComparison.Ordinal
                )
            )
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
                context.PublicApiModelTypes.Any(
                    predicate: (string modelType) =>
                        modelType.EndsWith(value: "." + candidate, comparisonType: StringComparison.Ordinal)
                )
        );
    }

    private static bool ImplementsMatchingInterface(EvaluationContext context)
    {
        if (context.ImplementedInterfaces.Count == 0)
        {
            return true;
        }

        string typeName = context.TypeName.Split(separator: ['.'])
            .Last();

        string expectedInterfaceName = "I" + typeName;

        return context.ImplementedInterfaces.Any(
            predicate: (string interfaceName) => interfaceName.Split(separator: ['.'])
            .Last() == expectedInterfaceName
        );
    }

    private static bool ContractContainsPublicMethods(EvaluationContext context)
    {
        return context.ImplementedInterfaces.Count == 0
            || context.PublicMethodNames.All(
                predicate: ((IEnumerable<string>)context.ContractMethodNames).Contains<string>
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
            Type = context.TypeName,
            LineNumber = (
                location is not null ? location.GetLineSpan().StartLinePosition.Line + 1 : context.LineNumber
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