// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Eventing.Models;

namespace cCoder.CodeAnalysis.Sample.Brokers.Events;

internal interface IEventBroker
{
    void ListenToEvent<T, TService>(string name, Func<TService, T, ValueTask> handler);

    ValueTask RaiseEventAsync<T>(string name, EventMessage<T> message);
}