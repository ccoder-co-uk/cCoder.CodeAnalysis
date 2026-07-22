// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Processings.Courses;
using cCoder.CodeAnalysis.Sample.Services.Processings.Students;
using cCoder.CodeAnalysis.Sample.Services.Processings.Teachers;
using cCoder.Eventing;

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.Events;

internal sealed partial class EventHandlerService(IEventHub eventHub) : IEventHandlerService
{
    public void ListenToAllEvents() =>
        TryCatch(operation: () =>
        {
            eventHub.ListenToEvent(
                name: "school_add",
                handler: (IStudentProcessingService service, School school) =>
                    service.AddOrUpdateStudentsAsync(students: school.Students, schoolId: school.Id)
            );

            eventHub.ListenToEvent(
                name: "school_add",
                handler: (ITeacherProcessingService service, School school) =>
                    service.AddOrUpdateTeachersAsync(teachers: school.Teachers, schoolId: school.Id)
            );

            eventHub.ListenToEvent(
                name: "school_add",
                handler: (ICourseProcessingService service, School school) =>
                    service.AddOrUpdateCoursesAsync(courses: school.Courses, schoolId: school.Id)
            );

            eventHub.ListenToEvent(
                name: "school_update",
                handler: (IStudentProcessingService service, School school) =>
                    service.AddOrUpdateStudentsAsync(students: school.Students, schoolId: school.Id)
            );

            eventHub.ListenToEvent(
                name: "school_update",
                handler: (ITeacherProcessingService service, School school) =>
                    service.AddOrUpdateTeachersAsync(teachers: school.Teachers, schoolId: school.Id)
            );

            eventHub.ListenToEvent(
                name: "school_update",
                handler: (ICourseProcessingService service, School school) =>
                    service.AddOrUpdateCoursesAsync(courses: school.Courses, schoolId: school.Id)
            );

            eventHub.ListenToEvent(
                name: "school_delete",
                handler: (IStudentProcessingService service, School school) =>
                    service.DeleteStudentsAsync(deletedStudents: school.Students)
            );

            eventHub.ListenToEvent(
                name: "school_delete",
                handler: (ITeacherProcessingService service, School school) =>
                    service.DeleteTeachersAsync(deletedTeachers: school.Teachers)
            );

            eventHub.ListenToEvent(
                name: "school_delete",
                handler: (ICourseProcessingService service, School school) =>
                    service.DeleteCoursesAsync(deletedCourses: school.Courses)
            );

            eventHub.ListenToEvent(
                name: "teacher_add",
                handler: (ICourseProcessingService service, Teacher teacher) =>
                    service.AddOrUpdateCoursesAsync(
                        courses: teacher.Courses,
                        schoolId: teacher.SchoolId,
                        teacherId: teacher.Id
                    )
            );

            eventHub.ListenToEvent(
                name: "teacher_update",
                handler: (ICourseProcessingService service, Teacher teacher) =>
                    service.AddOrUpdateCoursesAsync(
                        courses: teacher.Courses,
                        schoolId: teacher.SchoolId,
                        teacherId: teacher.Id
                    )
            );

            eventHub.ListenToEvent(
                name: "teacher_delete",
                handler: (ICourseProcessingService service, Teacher teacher) =>
                    service.DeleteCoursesAsync(deletedCourses: teacher.Courses)
            );

            eventHub.ListenToEvent(
                name: "course_add",
                handler: (IStudentProcessingService service, Course course) =>
                    service.AddOrUpdateStudentsAsync(students: course.Students, schoolId: course.SchoolId)
            );

            eventHub.ListenToEvent(
                name: "course_update",
                handler: (IStudentProcessingService service, Course course) =>
                    service.AddOrUpdateStudentsAsync(students: course.Students, schoolId: course.SchoolId)
            );
        });
}