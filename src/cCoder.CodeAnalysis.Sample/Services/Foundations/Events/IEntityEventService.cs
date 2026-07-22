// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.Events;

internal interface IEntityEventService
{
    ValueTask RaiseAddEventAsync<T>(string entityName, T entity);

    ValueTask RaiseUpdateEventAsync<T>(string entityName, T entity);

    ValueTask RaiseDeleteEventAsync<T>(string entityName, T entity);
}