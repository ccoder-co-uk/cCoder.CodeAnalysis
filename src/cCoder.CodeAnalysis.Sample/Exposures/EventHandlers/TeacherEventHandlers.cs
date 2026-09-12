// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Events;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Processings.Teachers;

namespace cCoder.CodeAnalysis.Sample.Exposures.EventHandlers;

internal sealed class TeacherEventHandlers(IEventBroker eventBroker) : ITeacherEventHandlers
{
    public void ListenToTeacherEvents()
    {
        eventBroker.ListenToEvent(
            name: "school_add",
            handler: (ITeacherProcessingService service, School school) =>
                service.AddOrUpdateTeachersAsync(teachers: school.Teachers, schoolId: school.Id));

        eventBroker.ListenToEvent(
            name: "school_update",
            handler: (ITeacherProcessingService service, School school) =>
                service.AddOrUpdateTeachersAsync(teachers: school.Teachers, schoolId: school.Id));

        eventBroker.ListenToEvent(
            name: "school_delete",
            handler: (ITeacherProcessingService service, School school) =>
                service.DeleteTeachersAsync(deletedTeachers: school.Teachers));
    }
}