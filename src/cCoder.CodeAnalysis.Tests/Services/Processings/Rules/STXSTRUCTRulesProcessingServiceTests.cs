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

public sealed class STXSTRUCTRulesProcessingServiceTests
{
    private readonly STXSTRUCTRulesProcessingService service = new();

    [Fact]
    public void MultipleTopLevelClassesShouldProduceDiagnostic()
    {
        EvaluationContext context = CreateContext(
            typeName: "Example.FirstClass",
            sourceCode:
                """
                namespace Example;

                public sealed class FirstClass
                {
                }

                internal sealed class SecondClass
                {
                }
                """);

        AnalysisItem[] items = service.Evaluate(context: context).ToArray();

        items.Should().ContainSingle(item => item.Code == "STXSTRUCT002", "");
    }

    [Fact]
    public void NestedClassShouldNotProduceDiagnostic()
    {
        EvaluationContext context = CreateContext(
            typeName: "Example.OuterClass",
            sourceCode:
                """
                namespace Example;

                public sealed class OuterClass
                {
                    private sealed class NestedClass
                    {
                    }
                }
                """);

        AnalysisItem[] items = service.Evaluate(context: context).ToArray();

        items.Should().NotContain(item => item.Code == "STXSTRUCT002", "");
    }

    [Fact]
    public void SingleTopLevelClassShouldNotProduceDiagnostic()
    {
        EvaluationContext context = CreateContext(
            typeName: "Example.OnlyClass",
            sourceCode:
                """
                namespace Example;

                public sealed class OnlyClass
                {
                }
                """);

        AnalysisItem[] items = service.Evaluate(context: context).ToArray();

        items.Should().NotContain(item => item.Code == "STXSTRUCT002", "");
    }

    private static EvaluationContext CreateContext(
        string typeName,
        string sourceCode)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            text: sourceCode,
            path: "Models/Example.cs");

        ClassDeclarationSyntax declaration = syntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .First();

        return new EvaluationContext
        {
            TypeName = typeName,
            StandardElementType = StandardElementType.Model,
            FilePath = syntaxTree.FilePath,
            SourceCode = sourceCode,
            Declarations = [declaration],
            Dependencies = [],
            ImplementedInterfaces = [],
            PublicMethodNames = [],
            ContractMethodNames = [],
            PublicMethodCallLineNumbers = [],
            PublicApiModelTypes = [],
            LocalDependencyTypeNames = [],
            ProjectTypeNames = [typeName],
            UsingNamespaces = [],
        };
    }
}
