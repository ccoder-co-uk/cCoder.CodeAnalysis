// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Events;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Events;

public sealed partial class EntityEventServiceTests
{
    private readonly Mock<IEventBroker> eventHubMock = new Mock<IEventBroker>();

    private EntityEventService CreateEntityEventService()
    {
        return new EntityEventService(eventHubMock.Object);
    }

    private static Student CreateStudent()
    {
        return new Student
        {
            Id = 7,
            FirstName = "Ada",
            LastName = "Lovelace",
        };
    }
}