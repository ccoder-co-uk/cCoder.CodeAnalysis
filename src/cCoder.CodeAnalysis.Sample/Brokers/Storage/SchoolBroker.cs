// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using Microsoft.EntityFrameworkCore;

namespace cCoder.CodeAnalysis.Sample.Brokers.Storage;

internal sealed class SchoolBroker(IDbContextFactory<SchoolContext> contextFactory) : ISchoolBroker
{
    public IQueryable<School> SelectAllSchools()
    {
        return contextFactory.CreateDbContext().Schools;
    }

    public async ValueTask<School> InsertSchoolAsync(School newSchool)
    {
        using SchoolContext context = contextFactory.CreateDbContext();
        School result = (await context.Schools.AddAsync(entity: newSchool)).Entity;
        await context.SaveChangesAsync();
        return result;
    }

    public async ValueTask<School> UpdateSchoolAsync(School updatedSchool)
    {
        using SchoolContext context = contextFactory.CreateDbContext();
        School result = context.Schools.Update(entity: updatedSchool).Entity;
        await context.SaveChangesAsync();
        return result;
    }

    public async ValueTask<int> DeleteSchoolAsync(School deletedSchool)
    {
        using SchoolContext context = contextFactory.CreateDbContext();
        context.Schools.Remove(entity: deletedSchool);
        return await context.SaveChangesAsync();
    }
}