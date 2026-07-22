// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;

namespace cCoder.CodeAnalysis.Sample.Exposures.EventHandlers;

internal sealed class SampleEventHandlers(IEventHandlerService service) : ISampleEventHandlers
{
    public void ListenToAllEvents()
    {
        service.ListenToAllEvents();
    }
}