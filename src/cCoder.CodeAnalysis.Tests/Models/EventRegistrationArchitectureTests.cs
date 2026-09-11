// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Models;
using cCoder.CodeAnalysis.Tests.Fixtures;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Tests.Models;

[Collection(SampleArchitectureCollection.Name)]
public sealed partial class EventRegistrationArchitectureTests(
    SampleArchitectureFixture fixture)
{
    private Architecture Architecture => fixture.Architecture;

    [Fact]
    public void EntityEventHandlers_WhenAnalysed_HaveOneBusinessBoundaryAndNoLayerViolations()
    {
        // Given
        string[] eventHandlerTypes =
        [
            "cCoder.CodeAnalysis.Sample.Exposures.EventHandlers.StudentEventHandlers",
            "cCoder.CodeAnalysis.Sample.Exposures.EventHandlers.TeacherEventHandlers",
            "cCoder.CodeAnalysis.Sample.Exposures.EventHandlers.CourseEventHandlers",
        ];

        Class[] eventHandlers = Architecture.Classes
            .Where(element => eventHandlerTypes.Contains(
                element.Name,
                StringComparer.Ordinal))
            .ToArray();

        // When
        AnalysisItem[] layerViolations = Architecture.AnalysisItems
            .Where(item => eventHandlerTypes.Contains(
                item.Type,
                StringComparer.Ordinal))
            .Where(item => item.Code is "STXF002" or "STXE003" or "STXE004" or "STXE005")
            .ToArray();

        string[][] processingBoundaries = eventHandlers
            .Select(eventHandler => eventHandler.Methods
                .SelectMany(method => method.Calls)
                .Where(call => call.StandardElementType ==
                    StandardElementType.ProcessingService)
                .Select(call => call.TypeName)
                .Distinct(StringComparer.Ordinal)
                .ToArray())
            .ToArray();

        // Then
        eventHandlers.Should().HaveCount(expected: 3, because: "");
        processingBoundaries.Should().OnlyContain(
            boundaries => boundaries.Length == 1);
        layerViolations.Should().BeEmpty();
    }

    [Fact]
    public void FormerFoundationEventHandler_WhenAnalysed_IsNoLongerPresent()
    {
        // Given
        const string formerEventHandler =
            "cCoder.CodeAnalysis.Sample.Services.Foundations.Events.EventHandlerService";

        // When
        Class? eventHandler = Architecture.Classes.SingleOrDefault(
            element => element.Name == formerEventHandler);

        // Then
        eventHandler.Should().BeNull();
        Architecture.AnalysisItems.Should().NotContain(item =>
            item.Type == formerEventHandler
            && item.Code == "STXF002");
    }
}