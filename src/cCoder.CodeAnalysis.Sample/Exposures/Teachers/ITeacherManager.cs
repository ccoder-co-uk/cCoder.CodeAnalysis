// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Exposures.Teachers;

public interface ITeacherManager
{
    Teacher? GetTeacher(int teacherId);

    IQueryable<Teacher> GetTeachers();

    ValueTask<Teacher> AddTeacherAsync(Teacher newTeacher);

    ValueTask<Teacher> UpdateTeacherAsync(Teacher updatedTeacher);

    ValueTask DeleteTeacherAsync(int teacherId);
}