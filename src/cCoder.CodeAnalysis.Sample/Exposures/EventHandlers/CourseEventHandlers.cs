// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Processings.Courses;
using cCoder.Eventing;

namespace cCoder.CodeAnalysis.Sample.Exposures.EventHandlers;

internal sealed class CourseEventHandlers(IEventHub eventHub) : ICourseEventHandlers
{
    public void ListenToCourseEvents()
    {
        eventHub.ListenToEvent(
            name: "school_add",
            handler: (ICourseProcessingService service, School school) =>
                service.AddOrUpdateCoursesAsync(courses: school.Courses, schoolId: school.Id));

        eventHub.ListenToEvent(
            name: "school_update",
            handler: (ICourseProcessingService service, School school) =>
                service.AddOrUpdateCoursesAsync(courses: school.Courses, schoolId: school.Id));

        eventHub.ListenToEvent(
            name: "school_delete",
            handler: (ICourseProcessingService service, School school) =>
                service.DeleteCoursesAsync(deletedCourses: school.Courses));

        eventHub.ListenToEvent(
            name: "teacher_add",
            handler: (ICourseProcessingService service, Teacher teacher) =>
                service.AddOrUpdateCoursesAsync(
                    courses: teacher.Courses,
                    schoolId: teacher.SchoolId,
                    teacherId: teacher.Id));

        eventHub.ListenToEvent(
            name: "teacher_update",
            handler: (ICourseProcessingService service, Teacher teacher) =>
                service.AddOrUpdateCoursesAsync(
                    courses: teacher.Courses,
                    schoolId: teacher.SchoolId,
                    teacherId: teacher.Id));

        eventHub.ListenToEvent(
            name: "teacher_delete",
            handler: (ICourseProcessingService service, Teacher teacher) =>
                service.DeleteCoursesAsync(deletedCourses: teacher.Courses));
    }
}