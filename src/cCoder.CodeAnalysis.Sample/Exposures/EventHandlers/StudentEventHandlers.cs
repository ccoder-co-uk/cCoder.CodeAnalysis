// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Processings.Students;
using cCoder.Eventing;

namespace cCoder.CodeAnalysis.Sample.Exposures.EventHandlers;

internal sealed class StudentEventHandlers(IEventHub eventHub) : IStudentEventHandlers
{
    public void ListenToStudentEvents()
    {
        eventHub.ListenToEvent(
            name: "school_add",
            handler: (IStudentProcessingService service, School school) =>
                service.AddOrUpdateStudentsAsync(students: school.Students, schoolId: school.Id));

        eventHub.ListenToEvent(
            name: "school_update",
            handler: (IStudentProcessingService service, School school) =>
                service.AddOrUpdateStudentsAsync(students: school.Students, schoolId: school.Id));

        eventHub.ListenToEvent(
            name: "school_delete",
            handler: (IStudentProcessingService service, School school) =>
                service.DeleteStudentsAsync(deletedStudents: school.Students));

        eventHub.ListenToEvent(
            name: "course_add",
            handler: (IStudentProcessingService service, Course course) =>
                service.AddOrUpdateStudentsAsync(students: course.Students, schoolId: course.SchoolId));

        eventHub.ListenToEvent(
            name: "course_update",
            handler: (IStudentProcessingService service, Course course) =>
                service.AddOrUpdateStudentsAsync(students: course.Students, schoolId: course.SchoolId));
    }
}