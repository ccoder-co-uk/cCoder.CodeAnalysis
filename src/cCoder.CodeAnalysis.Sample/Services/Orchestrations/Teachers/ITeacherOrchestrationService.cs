// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Orchestrations.Teachers;

internal interface ITeacherOrchestrationService
{
    Teacher? GetTeacher(int teacherId);

    IQueryable<Teacher> GetTeachers();

    ValueTask<Teacher> AddTeacherAsync(Teacher newTeacher);

    ValueTask<Teacher> UpdateTeacherAsync(Teacher updatedTeacher);

    ValueTask DeleteTeacherAsync(int teacherId);
}