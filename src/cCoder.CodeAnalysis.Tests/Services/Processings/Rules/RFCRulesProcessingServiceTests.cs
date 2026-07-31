// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------
using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class RFCRulesProcessingServiceTests
{
    private readonly RFCRulesProcessingService service = new();

    [Theory]
    [InlineData(
        "public IActionResult Post([FromBody] App app) => Ok(app);",
        "RFC0001")]
    [InlineData(
        "public IActionResult Delete(int key) => Ok();",
        "RFC0002")]
    [InlineData(
        "public IActionResult Get() => NoContent();",
        "RFC0003")]
    [InlineData(
        "public IActionResult Put(int key, App app) => NoContent();",
        "RFC0004")]
    [InlineData(
        "public IActionResult Patch(int key, App app) => NoContent();",
        "RFC0004")]
    public void EvaluateShouldRejectNonCompliantCrudResult(
        string method,
        string expectedCode)
    {
        EvaluationContext context = CreateContext(method: method);

        service.Evaluate(context: context)
            .Should()
            .ContainSingle(item => item.Code == expectedCode);
    }

    [Theory]
    [InlineData(
        "public IActionResult Post([FromBody] App app) => Created(app);")]
    [InlineData(
        "public IActionResult Delete(int key) => NoContent();")]
    [InlineData(
        "public IActionResult Get() => Ok(new App());")]
    [InlineData(
        "public IActionResult Put(int key, App app) => Ok(app);")]
    [InlineData(
        "public IActionResult Patch(int key, App app) => Updated(app);")]
    public void EvaluateShouldAllowCompliantCrudResult(string method)
    {
        EvaluationContext context = CreateContext(method: method);

        service.Evaluate(context: context)
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void EvaluateShouldAllowActionStylePost()
    {
        EvaluationContext context = CreateContext(
            method:
                "public IActionResult PostUpdateOrder(App app) => Ok();");

        service.Evaluate(context: context)
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void EvaluateShouldIgnoreNonODataController()
    {
        TypeDeclarationSyntax declaration = ParseDeclaration(
            """
            public class AppController : Controller
            {
                public IActionResult Post([FromBody] App app) => Ok(app);
            }
            """);

        EvaluationContext context = new()
        {
            TypeName = "Example.AppController",
            Declarations = [declaration],
        };

        service.Evaluate(context: context)
            .Should()
            .BeEmpty();
    }

    private static EvaluationContext CreateContext(string method)
    {
        TypeDeclarationSyntax declaration = ParseDeclaration(
            $$"""
            public class AppController : ODataController
            {
                {{method}}
            }
            """);

        return new EvaluationContext
        {
            TypeName = "Example.AppController",
            Declarations = [declaration],
        };
    }

    private static TypeDeclarationSyntax ParseDeclaration(string source) =>
        CSharpSyntaxTree.ParseText(text: source)
            .GetRoot()
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Single();
}
