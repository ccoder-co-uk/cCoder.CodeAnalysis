// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.Teachers;

internal sealed partial class TeacherService(ITeacherBroker teacherBroker) : ITeacherService
{
    public Teacher? GetTeacher(int teacherId) =>
        TryCatch(operation: () =>
        {
            ValidateTeacherOnGet(teacherId: teacherId);

            return teacherBroker.SelectAllTeachers()
                .FirstOrDefault(predicate: (Teacher item) => item.Id == teacherId);
        });

    public IQueryable<Teacher> GetTeachers()
    {
        return TryCatch(operation: () => teacherBroker.SelectAllTeachers());
    }

    public ValueTask<Teacher> AddTeacherAsync(Teacher newTeacher) =>
        TryCatch<Teacher>(operation: async () =>
        {
            ValidateTeacherOnAdd(newTeacher: newTeacher);
            Teacher storageTeacher = WithoutRelationships(teacher: newTeacher);
            await teacherBroker.InsertTeacherAsync(newTeacher: storageTeacher);
            return storageTeacher;
        });

    public ValueTask<Teacher> UpdateTeacherAsync(Teacher updatedTeacher) =>
        TryCatch<Teacher>(operation: async () =>
        {
            ValidateTeacherOnUpdate(updatedTeacher: updatedTeacher);
            Teacher storageTeacher = WithoutRelationships(teacher: updatedTeacher);
            await teacherBroker.UpdateTeacherAsync(updatedTeacher: storageTeacher);
            return storageTeacher;
        });

    public ValueTask DeleteTeacherAsync(int teacherId) =>
        TryCatch(operation: async () =>
        {
            ValidateTeacherOnDelete(teacherId: teacherId);

            Teacher? deletedTeacher = teacherBroker
                .SelectAllTeachers()
                .FirstOrDefault(predicate: (Teacher item) => item.Id == teacherId);

            if (deletedTeacher != null)
            {
                await teacherBroker.DeleteTeacherAsync(deletedTeacher: deletedTeacher);
            }
        });

    private static Teacher WithoutRelationships(Teacher teacher)
    {
        return new Teacher
        {
            Id = teacher.Id,
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            SchoolId = teacher.SchoolId,
        };
    }
}