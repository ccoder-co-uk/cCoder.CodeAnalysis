// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Exposures.EventHandlers;
using cCoder.CodeAnalysis.Sample.Services.Processings.Students;
using cCoder.Eventing;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Exposures.EventHandlers;

public sealed partial class StudentEventHandlersTests
{
    [Fact]
    public void ListenToStudentEventsRegistersEveryStudentHandler()
    {
        // Given
        Mock<IEventHub> eventHubMock = new();
        StudentEventHandlers eventHandlers = new(eventHub: eventHubMock.Object);

        // When
        eventHandlers.ListenToStudentEvents();

        // Then
        eventHubMock.Invocations
            .Should()
            .HaveCount(expected: 5, because: "");

        eventHubMock.Invocations
            .Select(selector: invocation => invocation.Arguments[0])
            .Should()
            .BeEquivalentTo(
                expectation:
                [
                    "school_add",
                    "school_update",
                    "school_delete",
                    "course_add",
                    "course_update",
                ],
                config: options => options.WithStrictOrdering(),
                because: "");

        eventHubMock.Invocations
            .Should()
            .OnlyContain(
                predicate: invocation => invocation.Method.GetGenericArguments()[1] ==
                    typeof(IStudentProcessingService),
                because: "");
    }
}