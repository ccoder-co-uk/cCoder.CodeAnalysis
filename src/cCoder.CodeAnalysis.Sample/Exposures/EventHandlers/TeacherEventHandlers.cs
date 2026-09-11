// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Processings.Teachers;
using cCoder.Eventing;

namespace cCoder.CodeAnalysis.Sample.Exposures.EventHandlers;

internal sealed class TeacherEventHandlers(IEventHub eventHub) : ITeacherEventHandlers
{
    public void ListenToTeacherEvents()
    {
        eventHub.ListenToEvent(
            name: "school_add",
            handler: (ITeacherProcessingService service, School school) =>
                service.AddOrUpdateTeachersAsync(teachers: school.Teachers, schoolId: school.Id));

        eventHub.ListenToEvent(
            name: "school_update",
            handler: (ITeacherProcessingService service, School school) =>
                service.AddOrUpdateTeachersAsync(teachers: school.Teachers, schoolId: school.Id));

        eventHub.ListenToEvent(
            name: "school_delete",
            handler: (ITeacherProcessingService service, School school) =>
                service.DeleteTeachersAsync(deletedTeachers: school.Teachers));
    }
}