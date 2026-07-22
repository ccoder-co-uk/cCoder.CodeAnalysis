// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Exposures.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Brokers.Storage;

internal sealed class SchoolBroker(ISchoolContextFactory contextFactory) : ISchoolBroker
{
    public IQueryable<School> SelectAllSchools()
    {
        return contextFactory.CreateSchoolContext().Schools;
    }

    public async ValueTask<School> InsertSchoolAsync(School newSchool)
    {
        using SchoolContext context = contextFactory.CreateSchoolContext();
        School result = (await context.Schools.AddAsync(entity: newSchool)).Entity;
        await context.SaveChangesAsync();
        return result;
    }

    public async ValueTask<School> UpdateSchoolAsync(School updatedSchool)
    {
        using SchoolContext context = contextFactory.CreateSchoolContext();
        School result = context.Schools.Update(entity: updatedSchool).Entity;
        await context.SaveChangesAsync();
        return result;
    }

    public async ValueTask<int> DeleteSchoolAsync(School deletedSchool)
    {
        using SchoolContext context = contextFactory.CreateSchoolContext();
        context.Schools.Remove(entity: deletedSchool);
        return await context.SaveChangesAsync();
    }
}