// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Eventing;
using cCoder.Eventing.Models;

namespace cCoder.CodeAnalysis.Sample.Brokers.Events;

internal sealed class EventBroker(IEventHub eventHub) : IEventBroker
{
    public void ListenToEvent<T, TService>(string name, Func<TService, T, ValueTask> handler) =>
        eventHub.ListenToEvent(name: name, handler: handler);

    public ValueTask RaiseEventAsync<T>(string name, EventMessage<T> message) =>
        eventHub.RaiseEventAsync(name: name, message: message);
}