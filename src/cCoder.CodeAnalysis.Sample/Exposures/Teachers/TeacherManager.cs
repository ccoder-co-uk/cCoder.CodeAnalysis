// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Teachers;

namespace cCoder.CodeAnalysis.Sample.Exposures.Teachers;

internal sealed class TeacherManager(ITeacherOrchestrationService service) : ITeacherManager
{
    public Teacher? GetTeacher(int teacherId)
    {
        return service.GetTeacher(teacherId: teacherId);
    }

    public IQueryable<Teacher> GetTeachers()
    {
        return service.GetTeachers();
    }

    public ValueTask<Teacher> AddTeacherAsync(Teacher newTeacher)
    {
        return service.AddTeacherAsync(newTeacher: newTeacher);
    }

    public ValueTask<Teacher> UpdateTeacherAsync(Teacher updatedTeacher)
    {
        return service.UpdateTeacherAsync(updatedTeacher: updatedTeacher);
    }

    public ValueTask DeleteTeacherAsync(int teacherId)
    {
        return service.DeleteTeacherAsync(teacherId: teacherId);
    }
}