// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using Microsoft.EntityFrameworkCore;

namespace cCoder.CodeAnalysis.Sample.Brokers.Storage;

internal sealed class CourseBroker(IDbContextFactory<SchoolContext> contextFactory) : ICourseBroker
{
    public IQueryable<Course> SelectAllCourses()
    {
        return contextFactory.CreateDbContext().Courses;
    }

    public async ValueTask<Course> InsertCourseAsync(Course newCourse)
    {
        using SchoolContext context = contextFactory.CreateDbContext();
        Course result = (await context.Courses.AddAsync(entity: newCourse)).Entity;
        await context.SaveChangesAsync();
        return result;
    }

    public async ValueTask<Course> UpdateCourseAsync(Course updatedCourse)
    {
        using SchoolContext context = contextFactory.CreateDbContext();
        Course result = context.Courses.Update(entity: updatedCourse).Entity;
        await context.SaveChangesAsync();
        return result;
    }

    public async ValueTask<int> DeleteCourseAsync(Course deletedCourse)
    {
        using SchoolContext context = contextFactory.CreateDbContext();
        context.Courses.Remove(entity: deletedCourse);
        return await context.SaveChangesAsync();
    }
}