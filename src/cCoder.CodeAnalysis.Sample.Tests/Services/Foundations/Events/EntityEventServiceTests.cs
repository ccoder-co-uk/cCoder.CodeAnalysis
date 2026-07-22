// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using cCoder.Eventing;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Events;

public sealed partial class EntityEventServiceTests
{
    private readonly Mock<IEventHub> eventHubMock = new Mock<IEventHub>();

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