// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Events;
using cCoder.CodeAnalysis.Sample.Exposures.EventHandlers;
using cCoder.CodeAnalysis.Sample.Services.Processings.Courses;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Exposures.EventHandlers;

public sealed partial class CourseEventHandlersTests
{
    [Fact]
    public void ListenToCourseEventsRegistersEveryCourseHandler()
    {
        // Given
        Mock<IEventBroker> eventHubMock = new();
        CourseEventHandlers eventHandlers = new(eventBroker: eventHubMock.Object);

        // When
        eventHandlers.ListenToCourseEvents();

        // Then
        eventHubMock.Invocations
            .Should()
            .HaveCount(expected: 6, because: "");

        eventHubMock.Invocations
            .Select(selector: invocation => invocation.Arguments[0])
            .Should()
            .BeEquivalentTo(
                expectation:
                [
                    "school_add",
                    "school_update",
                    "school_delete",
                    "teacher_add",
                    "teacher_update",
                    "teacher_delete",
                ],
                config: options => options.WithStrictOrdering(),
                because: "");

        eventHubMock.Invocations
            .Should()
            .OnlyContain(
                predicate: invocation => invocation.Method.GetGenericArguments()[1] ==
                    typeof(ICourseProcessingService),
                because: "");
    }
}