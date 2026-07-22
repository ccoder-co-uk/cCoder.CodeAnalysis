// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Exposures.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Brokers.Storage;

internal sealed class TeacherBroker(ISchoolContextFactory contextFactory) : ITeacherBroker
{
    public IQueryable<Teacher> SelectAllTeachers()
    {
        return contextFactory.CreateSchoolContext().Teachers;
    }

    public async ValueTask<Teacher> InsertTeacherAsync(Teacher newTeacher)
    {
        using SchoolContext context = contextFactory.CreateSchoolContext();
        Teacher result = (await context.Teachers.AddAsync(entity: newTeacher)).Entity;
        await context.SaveChangesAsync();
        return result;
    }

    public async ValueTask<Teacher> UpdateTeacherAsync(Teacher updatedTeacher)
    {
        using SchoolContext context = contextFactory.CreateSchoolContext();
        Teacher result = context.Teachers.Update(entity: updatedTeacher).Entity;
        await context.SaveChangesAsync();
        return result;
    }

    public async ValueTask<int> DeleteTeacherAsync(Teacher deletedTeacher)
    {
        using SchoolContext context = contextFactory.CreateSchoolContext();
        context.Teachers.Remove(entity: deletedTeacher);
        return await context.SaveChangesAsync();
    }
}