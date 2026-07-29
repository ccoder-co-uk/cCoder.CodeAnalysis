// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Events;

public sealed partial class EventHandlerServiceTests
{
    [Fact]
    public void ListenToAllEventsRegistersEveryHandler()
    {
        // Given
        // When
        // Then
        EventHandlerService service = CreateEventHandlerService();
        service.ListenToAllEvents();

        ((IEnumerable<IInvocation>)eventHubMock.Invocations).Should()
            .HaveCount(expected: 14, because: "");
    }
}