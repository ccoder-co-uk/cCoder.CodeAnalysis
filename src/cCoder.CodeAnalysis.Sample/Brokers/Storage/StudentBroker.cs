// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Exposures.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Brokers.Storage;

internal sealed class StudentBroker(ISchoolContextFactory contextFactory) : IStudentBroker
{
    public IQueryable<Student> SelectAllStudents()
    {
        return contextFactory.CreateSchoolContext().Students;
    }

    public async ValueTask<Student> InsertStudentAsync(Student newStudent)
    {
        using SchoolContext context = contextFactory.CreateSchoolContext();
        Student result = (await context.Students.AddAsync(entity: newStudent)).Entity;
        await context.SaveChangesAsync();
        return result;
    }

    public async ValueTask<Student> UpdateStudentAsync(Student updatedStudent)
    {
        using SchoolContext context = contextFactory.CreateSchoolContext();
        Student result = context.Students.Update(entity: updatedStudent).Entity;
        await context.SaveChangesAsync();
        return result;
    }

    public async ValueTask<int> DeleteStudentAsync(Student deletedStudent)
    {
        using SchoolContext context = contextFactory.CreateSchoolContext();
        context.Students.Remove(entity: deletedStudent);
        return await context.SaveChangesAsync();
    }
}