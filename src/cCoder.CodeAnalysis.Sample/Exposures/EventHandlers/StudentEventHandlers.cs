// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Events;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Processings.Students;

namespace cCoder.CodeAnalysis.Sample.Exposures.EventHandlers;

internal sealed class StudentEventHandlers(IEventBroker eventBroker) : IStudentEventHandlers
{
    public void ListenToStudentEvents()
    {
        eventBroker.ListenToEvent(
            name: "school_add",
            handler: (IStudentProcessingService service, School school) =>
                service.AddOrUpdateStudentsAsync(students: school.Students, schoolId: school.Id));

        eventBroker.ListenToEvent(
            name: "school_update",
            handler: (IStudentProcessingService service, School school) =>
                service.AddOrUpdateStudentsAsync(students: school.Students, schoolId: school.Id));

        eventBroker.ListenToEvent(
            name: "school_delete",
            handler: (IStudentProcessingService service, School school) =>
                service.DeleteStudentsAsync(deletedStudents: school.Students));

        eventBroker.ListenToEvent(
            name: "course_add",
            handler: (IStudentProcessingService service, Course course) =>
                service.AddOrUpdateStudentsAsync(students: course.Students, schoolId: course.SchoolId));

        eventBroker.ListenToEvent(
            name: "course_update",
            handler: (IStudentProcessingService service, Course course) =>
                service.AddOrUpdateStudentsAsync(students: course.Students, schoolId: course.SchoolId));
    }
}