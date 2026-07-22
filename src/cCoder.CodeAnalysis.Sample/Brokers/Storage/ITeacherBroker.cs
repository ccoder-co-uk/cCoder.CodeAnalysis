// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Brokers.Storage;

internal interface ITeacherBroker
{
    IQueryable<Teacher> SelectAllTeachers();

    ValueTask<Teacher> InsertTeacherAsync(Teacher newTeacher);

    ValueTask<Teacher> UpdateTeacherAsync(Teacher updatedTeacher);

    ValueTask<int> DeleteTeacherAsync(Teacher deletedTeacher);
}