// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.ArchitectureModels;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.ArchitectureModels;

public sealed class ArchitectureModelQueriesProcessingServiceTests
{
    private readonly ArchitectureModelQueriesProcessingService service = new();

    [Fact]
    public void GetDependenciesShouldPreferAttachedModelFacts()
    {
        TypeDependency legacyDependency = new() { TypeName = "Legacy" };
        TypeDependency modelDependency = new() { TypeName = "Model" };
        EvaluationContext context = new()
        {
            Dependencies = [legacyDependency],
            ArchitectureElement = new Class
            {
                AnalysisDependencies = [modelDependency],
            },
        };

        service.GetDependencies(context: context).Should().ContainSingle()
            .Which.Should().BeSameAs(modelDependency);
    }

    [Fact]
    public void GetDependenciesShouldSupportLegacyContextDuringMigration()
    {
        TypeDependency dependency = new() { TypeName = "Legacy" };
        EvaluationContext context = new() { Dependencies = [dependency] };

        service.GetDependencies(context: context).Should().ContainSingle()
            .Which.Should().BeSameAs(dependency);
    }

    [Fact]
    public void SourceQueriesShouldPreferAttachedModelFacts()
    {
        TypeDeclarationSyntax modelDeclaration = CreateDeclaration(source: "class Model { }");
        TypeDeclarationSyntax legacyDeclaration = CreateDeclaration(source: "class Legacy { }");
        EvaluationContext context = new()
        {
            Declarations = [legacyDeclaration],
            FilePath = "Legacy.cs",
            SourceCode = "legacy",
            ProjectLineEnding = "\n",
            ArchitectureElement = new Class
            {
                AnalysisDeclarations = [modelDeclaration],
                AnalysisFilePath = "Model.cs",
                AnalysisSourceCode = "model",
                AnalysisProjectLineEnding = "\r\n",
            },
        };

        service.GetDeclarations(context: context).Should().ContainSingle()
            .Which.Should().BeSameAs(modelDeclaration);
        service.GetFilePath(context: context).Should().Be(expected: "Model.cs");
        service.GetSourceCode(context: context).Should().Be(expected: "model");
        service.GetProjectLineEnding(context: context).Should().Be(expected: "\r\n");
    }

    [Fact]
    public void SourceQueriesShouldSupportLegacyContextDuringMigration()
    {
        TypeDeclarationSyntax declaration = CreateDeclaration(source: "class Legacy { }");
        EvaluationContext context = new()
        {
            Declarations = [declaration],
            FilePath = "Legacy.cs",
            SourceCode = "legacy",
            ProjectLineEnding = "\n",
        };

        service.GetDeclarations(context: context).Should().ContainSingle()
            .Which.Should().BeSameAs(declaration);
        service.GetFilePath(context: context).Should().Be(expected: "Legacy.cs");
        service.GetSourceCode(context: context).Should().Be(expected: "legacy");
        service.GetProjectLineEnding(context: context).Should().Be(expected: "\n");
    }

    [Fact]
    public void GetReachableMethodsShouldFollowInternalCallsAndStopAtDependencyBoundary()
    {
        Method dependency = CreateMethod(id: "Dependency.Save");
        Method serviceMethod = CreateMethod(
            id: "Service.Update",
            calls:
            [
                new MethodCall
                {
                    MethodId = dependency.Id,
                    TypeName = "Database.Dependency",
                    IsDependencyBoundary = true,
                },
            ]);
        Method controller = CreateMethod(
            id: "Controller.Put",
            calls: [new MethodCall { MethodId = serviceMethod.Id, TypeName = "Service" }]);
        EvaluationContext context = CreateContext(controller, serviceMethod, dependency);

        service.GetReachableMethods(context: context, methodId: controller.Id)
            .Select(method => method.Id)
            .Should().Equal(controller.Id, serviceMethod.Id);
    }

    [Fact]
    public void CallsTypeMatchingShouldInspectTheReachableCallChain()
    {
        Method authorization = CreateMethod(id: "AuthorizationBroker.Authorize");
        Method serviceMethod = CreateMethod(
            id: "Service.Update",
            calls:
            [
                new MethodCall
                {
                    MethodId = authorization.Id,
                    TypeName = "Security.AuthorizationBroker",
                    IsDependencyBoundary = true,
                },
            ]);
        Method controller = CreateMethod(
            id: "Controller.Put",
            calls: [new MethodCall { MethodId = serviceMethod.Id, TypeName = "Service" }]);
        EvaluationContext context = CreateContext(controller, serviceMethod, authorization);

        service.CallsTypeMatching(
            context: context,
            methodId: controller.Id,
            typeNameFragment: "AuthorizationBroker").Should().BeTrue();
    }

    private static EvaluationContext CreateContext(params Method[] methods)
    {
        Class element = new() { AnalysisMethods = methods.ToList() };

        return new EvaluationContext
        {
            ArchitectureElement = element,
            ArchitectureModel = new Architecture { Classes = [element] },
        };
    }

    private static Method CreateMethod(string id, List<MethodCall>? calls = null) =>
        new()
        {
            Id = id,
            DirectCalls = calls ?? [],
            ThrowsExceptionTypes = [],
        };

    private static TypeDeclarationSyntax CreateDeclaration(string source) =>
        CSharpSyntaxTree.ParseText(text: source)
            .GetRoot()
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Single();
}
