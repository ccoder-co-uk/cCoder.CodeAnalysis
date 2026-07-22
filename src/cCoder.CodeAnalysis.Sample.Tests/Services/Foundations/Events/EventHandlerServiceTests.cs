// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using cCoder.Eventing;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Events;

public sealed partial class EventHandlerServiceTests
{
    private readonly Mock<IEventHub> eventHubMock = new Mock<IEventHub>();

    private EventHandlerService CreateEventHandlerService()
    {
        return new EventHandlerService(eventHubMock.Object);
    }
}