// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Events;

public sealed partial class EntityEventServiceTests
{
    [Fact]
    public async Task RaiseDeleteEventAsyncRaisesNamedEvent()
    {
        // Given
        // When
        // Then
        Student student = CreateStudent();
        EntityEventService service = CreateEntityEventService();
        await service.RaiseDeleteEventAsync(entityName:"student", entity:student);

        ((IEnumerable<IInvocation>)eventHubMock.Invocations).Should()
            .ContainSingle(because:"");

        eventHubMock.Invocations[0].Arguments[0].Should()
            .Be(expected:"student_delete", because:"");
    }
}