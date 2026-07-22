// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Eventing;
using cCoder.Eventing.Models;

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.Events;

internal sealed partial class EntityEventService(IEventHub eventHub) : IEntityEventService
{
    public ValueTask RaiseAddEventAsync<T>(string entityName, T entity) =>
        TryCatch(operation: () =>
        {
            Validate(inputs: [entityName, entity]);
            return RaiseAsync(eventName: entityName + "_add", entity: entity);
        });

    public ValueTask RaiseUpdateEventAsync<T>(string entityName, T entity) =>
        TryCatch(operation: () =>
        {
            Validate(inputs: [entityName, entity]);
            return RaiseAsync(eventName: entityName + "_update", entity: entity);
        });

    public ValueTask RaiseDeleteEventAsync<T>(string entityName, T entity) =>
        TryCatch(operation: () =>
        {
            Validate(inputs: [entityName, entity]);
            return RaiseAsync(eventName: entityName + "_delete", entity: entity);
        });

    private ValueTask RaiseAsync<T>(string eventName, T entity) =>
        eventHub.RaiseEventAsync(
            name: eventName,
            message: new EventMessage<T>
            {
                AuthInfo = new EventAuthInfo { SSOUserId = string.Empty },
                Data = entity,
            }
        );
}