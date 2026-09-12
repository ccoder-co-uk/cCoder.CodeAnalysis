// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using Microsoft.EntityFrameworkCore;

namespace cCoder.CodeAnalysis.Sample.Brokers.Storage;

internal sealed class StudentBroker(IDbContextFactory<SchoolContext> contextFactory) : IStudentBroker
{
    public IQueryable<Student> SelectAllStudents()
    {
        return contextFactory.CreateDbContext().Students;
    }

    public async ValueTask<Student> InsertStudentAsync(Student newStudent)
    {
        using SchoolContext context = contextFactory.CreateDbContext();
        Student result = (await context.Students.AddAsync(entity: newStudent)).Entity;
        await context.SaveChangesAsync();
        return result;
    }

    public async ValueTask<Student> UpdateStudentAsync(Student updatedStudent)
    {
        using SchoolContext context = contextFactory.CreateDbContext();
        Student result = context.Students.Update(entity: updatedStudent).Entity;
        await context.SaveChangesAsync();
        return result;
    }

    public async ValueTask<int> DeleteStudentAsync(Student deletedStudent)
    {
        using SchoolContext context = contextFactory.CreateDbContext();
        context.Students.Remove(entity: deletedStudent);
        return await context.SaveChangesAsync();
    }
}