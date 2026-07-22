// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Exposures.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Brokers.Storage;

internal sealed class CourseBroker(ISchoolContextFactory contextFactory) : ICourseBroker
{
    public IQueryable<Course> SelectAllCourses()
    {
        return contextFactory.CreateSchoolContext().Courses;
    }

    public async ValueTask<Course> InsertCourseAsync(Course newCourse)
    {
        using SchoolContext context = contextFactory.CreateSchoolContext();
        Course result = (await context.Courses.AddAsync(entity: newCourse)).Entity;
        await context.SaveChangesAsync();
        return result;
    }

    public async ValueTask<Course> UpdateCourseAsync(Course updatedCourse)
    {
        using SchoolContext context = contextFactory.CreateSchoolContext();
        Course result = context.Courses.Update(entity: updatedCourse).Entity;
        await context.SaveChangesAsync();
        return result;
    }

    public async ValueTask<int> DeleteCourseAsync(Course deletedCourse)
    {
        using SchoolContext context = contextFactory.CreateSchoolContext();
        context.Courses.Remove(entity: deletedCourse);
        return await context.SaveChangesAsync();
    }
}