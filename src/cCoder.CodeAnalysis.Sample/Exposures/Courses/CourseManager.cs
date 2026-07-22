// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Courses;

namespace cCoder.CodeAnalysis.Sample.Exposures.Courses;

internal sealed class CourseManager(ICourseOrchestrationService service) : ICourseManager
{
    public Course? GetCourse(int courseId)
    {
        return service.GetCourse(courseId: courseId);
    }

    public IQueryable<Course> GetCourses()
    {
        return service.GetCourses();
    }

    public ValueTask<Course> AddCourseAsync(Course newCourse)
    {
        return service.AddCourseAsync(newCourse: newCourse);
    }

    public ValueTask<Course> UpdateCourseAsync(Course updatedCourse)
    {
        return service.UpdateCourseAsync(updatedCourse: updatedCourse);
    }

    public ValueTask DeleteCourseAsync(int courseId)
    {
        return service.DeleteCourseAsync(courseId: courseId);
    }
}