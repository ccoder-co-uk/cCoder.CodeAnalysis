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

    [Fact]
    public void PublicServiceContractShouldProduceDiagnostic()
    {
        EvaluationContext context = CreateInterfaceContext(
            typeName: "Example.IStudentProcessingService",
            standardElementType:
                StandardElementType.ProcessingService,
            sourceCode:
                """
                namespace Example;

                public interface IStudentProcessingService
                {
                }
                """);

        AnalysisItem[] items =
            service.Evaluate(context: context).ToArray();

        items.Should()
            .ContainSingle(
                predicate: item =>
                    item.Code == "STXSTRUCT003",
                because: "");
    }

    [Fact]
    public void InternalServiceContractShouldNotProduceDiagnostic()
    {
        EvaluationContext context = CreateInterfaceContext(
            typeName: "Example.IStudentProcessingService",
            standardElementType:
                StandardElementType.ProcessingService,
            sourceCode:
                """
                namespace Example;

                internal interface IStudentProcessingService
                {
                }
                """);

        AnalysisItem[] items =
            service.Evaluate(context: context).ToArray();

        items.Should()
            .NotContain(
                predicate: item =>
                    item.Code == "STXSTRUCT003",
                because: "");
    }

    [Theory]
    [InlineData("Project/Controllers/StudentController.cs")]
    [InlineData("Project/Middleware/ErrorMiddleware.cs")]
    [InlineData("Project/Middlewares/ErrorMiddleware.cs")]
    [InlineData("Project/Exposures/HttpEndpoint.cs")]
    public void HttpExposureShouldAllowHttpBoundaryFolders(string filePath)
    {
        EvaluationContext context = CreateContext(
            typeName: "Example.HttpEndpoint",
            sourceCode: "public sealed class HttpEndpoint { }",
            filePath: filePath,
            standardElementType: StandardElementType.HttpExposure);

        service.Evaluate(context)
            .Should().NotContain(item => item.Code == "STXSTRUCT001");
    }

    [Fact]
    public void HttpExposureShouldRejectNonHttpFolder()
    {
        EvaluationContext context = CreateContext(
            typeName: "Example.HttpEndpoint",
            sourceCode: "public sealed class HttpEndpoint { }",
            filePath: "Models/HttpEndpoint.cs",
            standardElementType: StandardElementType.HttpExposure);

        service.Evaluate(context)
            .Should().ContainSingle(item => item.Code == "STXSTRUCT001");
    }

    private static EvaluationContext CreateContext(
        string typeName,
        string sourceCode,
        string filePath = "Models/Example.cs",
        StandardElementType standardElementType = StandardElementType.Model)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            text: sourceCode,
            path: filePath);

        ClassDeclarationSyntax declaration = syntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .First();

        ClassDeclarationSyntax[] topLevelClasses = syntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(candidate => !candidate.Ancestors().OfType<TypeDeclarationSyntax>().Any())
            .ToArray();

        Class element = new Class
        {
            Name = typeName,
            StandardElementType = standardElementType,
            AnalysisFilePath = syntaxTree.FilePath,
            AnalysisSourceCode = sourceCode,
            AnalysisDeclarations = [declaration],
            AnalysisSourceFileTopLevelClassCount = topLevelClasses.Length,
            AnalysisIsPrimaryTopLevelClassInFile =
                declaration.SpanStart == topLevelClasses[0].SpanStart,
        };

        Architecture architecture = new Architecture { Classes = [element] };

        return new EvaluationContext
        {
            ArchitectureModel = architecture,
            ArchitectureElement = element,
        };
    }

    private static EvaluationContext CreateInterfaceContext(
        string typeName,
        StandardElementType standardElementType,
        string sourceCode)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
            text: sourceCode,
            path: "Services/Processings/Example.cs");

        InterfaceDeclarationSyntax declaration = syntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<InterfaceDeclarationSyntax>()
            .First();

        Class element = new Class
        {
            Name = typeName,
            StandardElementType = standardElementType,
            Kind = ArchitectureTypeKind.Interface,
            AnalysisFilePath = syntaxTree.FilePath,
            AnalysisSourceCode = sourceCode,
            AnalysisDeclarations = [declaration],
        };

        Architecture architecture = new Architecture { Classes = [element] };

        return new EvaluationContext
        {
            ArchitectureModel = architecture,
            ArchitectureElement = element,
        };
    }
}
