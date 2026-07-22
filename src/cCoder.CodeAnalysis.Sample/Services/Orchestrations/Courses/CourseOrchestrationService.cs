// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Courses;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;

namespace cCoder.CodeAnalysis.Sample.Services.Orchestrations.Courses;

internal sealed partial class CourseOrchestrationService(ICourseService courseService, IEntityEventService eventService)
    : ICourseOrchestrationService
{
    public Course? GetCourse(int courseId) =>
        TryCatch(operation: () =>
        {
            Validate(inputs: courseId);
            return courseService.GetCourse(courseId: courseId);
        });

    public IQueryable<Course> GetCourses()
    {
        return TryCatch(operation: () => courseService.GetCourses());
    }

    public ValueTask<Course> AddCourseAsync(Course newCourse) =>
        TryCatch<Course>(operation: async () =>
        {
            Validate(inputs: newCourse);
            Course result = await courseService.AddCourseAsync(newCourse: WithoutRelationships(course: newCourse));
            newCourse.Id = result.Id;
            await eventService.RaiseAddEventAsync(entityName: "newCourse", entity: newCourse);
            return newCourse;
        });

    public ValueTask<Course> UpdateCourseAsync(Course updatedCourse) =>
        TryCatch<Course>(operation: async () =>
        {
            Validate(inputs: updatedCourse);
            await courseService.UpdateCourseAsync(updatedCourse: WithoutRelationships(course: updatedCourse));
            await eventService.RaiseUpdateEventAsync(entityName: "updatedCourse", entity: updatedCourse);
            return updatedCourse;
        });

    public ValueTask DeleteCourseAsync(int courseId) =>
        TryCatch(operation: async () =>
        {
            Validate(inputs: courseId);
            Course? updatedCourse = courseService.GetCourse(courseId: courseId);

            if (updatedCourse != null)
            {
                await eventService.RaiseDeleteEventAsync(entityName: "updatedCourse", entity: updatedCourse);
                await courseService.DeleteCourseAsync(courseId: courseId);
            }
        });

    private static Course WithoutRelationships(Course course)
    {
        return new Course
        {
            Id = course.Id,
            Name = course.Name,
            SchoolId = course.SchoolId,
            TeacherId = course.TeacherId,
        };
    }
}