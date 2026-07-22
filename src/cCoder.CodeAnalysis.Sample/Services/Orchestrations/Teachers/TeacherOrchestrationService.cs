// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Teachers;

namespace cCoder.CodeAnalysis.Sample.Services.Orchestrations.Teachers;

internal sealed partial class TeacherOrchestrationService(
    ITeacherService teacherService,
    IEntityEventService eventService
) : ITeacherOrchestrationService
{
    public Teacher? GetTeacher(int teacherId) =>
        TryCatch(operation: () =>
        {
            Validate(inputs: teacherId);
            return teacherService.GetTeacher(teacherId: teacherId);
        });

    public IQueryable<Teacher> GetTeachers()
    {
        return TryCatch(operation: () => teacherService.GetTeachers());
    }

    public ValueTask<Teacher> AddTeacherAsync(Teacher newTeacher) =>
        TryCatch<Teacher>(operation: async () =>
        {
            Validate(inputs: newTeacher);

            Teacher result = await teacherService.AddTeacherAsync(
                newTeacher: WithoutRelationships(teacher: newTeacher)
            );

            newTeacher.Id = result.Id;
            await eventService.RaiseAddEventAsync(entityName: "newTeacher", entity: newTeacher);
            return newTeacher;
        });

    public ValueTask<Teacher> UpdateTeacherAsync(Teacher updatedTeacher) =>
        TryCatch<Teacher>(operation: async () =>
        {
            Validate(inputs: updatedTeacher);
            await teacherService.UpdateTeacherAsync(updatedTeacher: WithoutRelationships(teacher: updatedTeacher));
            await eventService.RaiseUpdateEventAsync(entityName: "updatedTeacher", entity: updatedTeacher);
            return updatedTeacher;
        });

    public ValueTask DeleteTeacherAsync(int teacherId) =>
        TryCatch(operation: async () =>
        {
            Validate(inputs: teacherId);
            Teacher? updatedTeacher = teacherService.GetTeacher(teacherId: teacherId);

            if (updatedTeacher != null)
            {
                await eventService.RaiseDeleteEventAsync(entityName: "updatedTeacher", entity: updatedTeacher);
                await teacherService.DeleteTeacherAsync(teacherId: teacherId);
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