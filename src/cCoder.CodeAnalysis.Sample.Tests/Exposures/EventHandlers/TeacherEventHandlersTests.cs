// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Events;
using cCoder.CodeAnalysis.Sample.Exposures.EventHandlers;
using cCoder.CodeAnalysis.Sample.Services.Processings.Teachers;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Exposures.EventHandlers;

public sealed partial class TeacherEventHandlersTests
{
    [Fact]
    public void ListenToTeacherEventsRegistersEveryTeacherHandler()
    {
        // Given
        Mock<IEventBroker> eventHubMock = new();
        TeacherEventHandlers eventHandlers = new(eventBroker: eventHubMock.Object);

        // When
        eventHandlers.ListenToTeacherEvents();

        // Then
        eventHubMock.Invocations
            .Should()
            .HaveCount(expected: 3, because: "");

        eventHubMock.Invocations
            .Select(selector: invocation => invocation.Arguments[0])
            .Should()
            .BeEquivalentTo(
                expectation: ["school_add", "school_update", "school_delete"],
                config: options => options.WithStrictOrdering(),
                because: "");

        eventHubMock.Invocations
            .Should()
            .OnlyContain(
                predicate: invocation => invocation.Method.GetGenericArguments()[1] ==
                    typeof(ITeacherProcessingService),
                because: "");
    }
}