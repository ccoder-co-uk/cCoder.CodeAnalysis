// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class ExceptionHandlingRuleTests
{
    private static EvaluationContext CreateEvaluationContext(string catchClauses)
    {
        string source =
            "internal sealed partial class ExampleService\r\n{\r\n    private static void TryCatch(Action operation)\r\n    {\r\n        try\r\n        {\r\n            operation();\r\n        }\r\n        "
            + catchClauses
            + "\r\n    }\r\n}";
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, null, "ExampleService.Exceptions.cs");
        TypeDeclarationSyntax declaration = syntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Single();
        Class element = new()
        {
            Name = "ExampleService",
            StandardElementType = StandardElementType.FoundationService,
            LineNumber = 1,
            Properties = [],
            Methods = [],
            AnalysisDeclarations = [declaration],
            AnalysisImplementedInterfaces = [],
        };

        return new EvaluationContext
        {
            ArchitectureElement = element,
            ArchitectureModel = new Architecture
            {
                Project = new ProjectMetadata { AssemblyName = "Example" },
                Classes = [element],
            },
        };
    }

    private static AnalysisItem[] Evaluate(string catchClauses)
    {
        EvaluationContext context = CreateEvaluationContext(catchClauses);
        STXEXRulesProcessingService service =
            new STXEXRulesProcessingService();
        return service.Evaluate(context).ToArray();
    }

    [Theory]
    [InlineData(new object[] { "STXEX001" })]
    [InlineData(new object[] { "STXEX002" })]
    [InlineData(new object[] { "STXEX003" })]
    public void MissingExceptionCategoryEvaluatesAsExpected(string expectedCode)
    {
        AnalysisItem[] analysisItems = Evaluate("catch { throw; }");
        ((IEnumerable<AnalysisItem>)analysisItems)
            .Should()
            .ContainSingle((AnalysisItem item) => item.Code == expectedCode, "");
    }

    [Fact]
    public void WrappedExceptionCategoriesEvaluateAsExpected()
    {
        AnalysisItem[] analysisItems = Evaluate(
            "catch (ArgumentException exception)\r\n{\r\n    throw new ExampleValidationException(exception);\r\n}\r\ncatch (ExampleDependencyException exception)\r\n{\r\n    throw new ExampleDependencyServiceException(exception);\r\n}\r\ncatch (Exception exception)\r\n{\r\n    throw new ExampleServiceException(exception);\r\n}"
        );
        ((IEnumerable<AnalysisItem>)analysisItems)
            .Should()
            .NotContain((AnalysisItem item) => item.Code.StartsWith("STXEX", StringComparison.Ordinal), "");
    }
}
