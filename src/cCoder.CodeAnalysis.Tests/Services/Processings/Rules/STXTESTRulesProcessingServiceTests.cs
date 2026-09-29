// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Services.Processings.Rules;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Services.Processings.Rules;

public sealed class STXTESTRulesProcessingServiceTests
{
    [Fact]
    public void ArchitectureSuite_WhenOwnedByDomainUnitProject_IsReported()
    {
        // Given
        EvaluationContext context = CreateContext(
            projectName: "cCoder.ContentManagement.Tests",
            filePath: "Architecture/ExposureBoundaryArchitectureTests.cs",
            typeName: "ExposureBoundaryArchitectureTests");
        STXTESTRulesProcessingService service = new();

        // When
        AnalysisItem[] results = service.Evaluate(context: context).ToArray();

        // Then
        results.Should().ContainSingle(item => item.Code == "STXTEST007");
    }

    [Fact]
    public void BrokerSuite_WhenOwnedByDomainUnitProject_IsReported()
    {
        // Given
        EvaluationContext context = CreateContext(
            projectName: "cCoder.ContentManagement.Tests",
            filePath: "Brokers/JsonBrokerTests.cs",
            typeName: "JsonBrokerTests");
        STXTESTRulesProcessingService service = new();

        // When
        AnalysisItem[] results = service.Evaluate(context: context).ToArray();

        // Then
        results.Should().ContainSingle(item => item.Code == "STXTEST008");
    }

    [Fact]
    public void ExposureSuite_WhenOwnedByDomainUnitProject_IsReported()
    {
        // Given
        EvaluationContext context = CreateContext(
            projectName: "cCoder.ContentManagement.Tests",
            filePath: "Exposures/TemplateManagerTests.cs",
            typeName: "TemplateManagerTests");
        STXTESTRulesProcessingService service = new();

        // When
        AnalysisItem[] results = service.Evaluate(context: context).ToArray();

        // Then
        results.Should().ContainSingle(item => item.Code == "STXTEST009");
    }

    [Theory]
    [InlineData("cCoder.ContentManagement.AcceptanceTests")]
    [InlineData("cCoder.ContentManagement.IntegrationTests")]
    public void BoundarySuite_WhenOwnedByNonUnitProject_IsNotReported(
        string projectName)
    {
        // Given
        EvaluationContext context = CreateContext(
            projectName: projectName,
            filePath: "Exposures/TemplateManagerTests.cs",
            typeName: "TemplateManagerTests");
        STXTESTRulesProcessingService service = new();

        // When
        AnalysisItem[] results = service.Evaluate(context: context).ToArray();

        // Then
        results.Should().NotContain(item =>
            item.Code == "STXTEST007"
            || item.Code == "STXTEST008"
            || item.Code == "STXTEST009");
    }

    private static EvaluationContext CreateContext(
        string projectName,
        string filePath,
        string typeName)
    {
        TypeAnalysisFacts facts = new()
        {
            ProjectName = projectName,
            FilePath = filePath,
            AllDeclarationsArePartial = true,
            Methods =
            [
                new MethodAnalysisFacts
                {
                    Name = "ShouldBehaveAsExpected",
                    IsTest = true,
                    IsFact = true,
                    HasGivenWhenThenComments = true
                }
            ]
        };

        Class architectureElement = new()
        {
            Name = $"Example.Tests.{typeName}",
            StandardElementType = StandardElementType.Test,
            AnalysisTypeFacts = facts
        };

        return new EvaluationContext
        {
            ArchitectureElement = architectureElement,
            ArchitectureModel = new Architecture
            {
                Project = new ProjectMetadata { AssemblyName = projectName },
                Classes = [architectureElement]
            }
        };
    }
}
