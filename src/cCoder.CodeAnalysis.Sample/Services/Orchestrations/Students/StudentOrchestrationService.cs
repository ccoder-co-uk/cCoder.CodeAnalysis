// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;

namespace cCoder.CodeAnalysis.Sample.Services.Orchestrations.Students;

internal sealed partial class StudentOrchestrationService(
    IStudentService studentService,
    IEntityEventService eventService
) : IStudentOrchestrationService
{
    public Student? GetStudent(int studentId) =>
        TryCatch(operation: () =>
        {
            Validate(inputs: studentId);
            return studentService.GetStudent(studentId: studentId);
        });

    public IQueryable<Student> GetStudents()
    {
        return TryCatch(operation: () => studentService.GetStudents());
    }

    public ValueTask<Student> AddStudentAsync(Student newStudent) =>
        TryCatch<Student>(operation: async () =>
        {
            Validate(inputs: newStudent);

            Student result = await studentService.AddStudentAsync(
                newStudent: WithoutRelationships(student: newStudent)
            );

            newStudent.Id = result.Id;
            await eventService.RaiseAddEventAsync(entityName: "newStudent", entity: newStudent);
            return newStudent;
        });

    public ValueTask<Student> UpdateStudentAsync(Student updatedStudent) =>
        TryCatch<Student>(operation: async () =>
        {
            Validate(inputs: updatedStudent);
            await studentService.UpdateStudentAsync(updatedStudent: WithoutRelationships(student: updatedStudent));
            await eventService.RaiseUpdateEventAsync(entityName: "updatedStudent", entity: updatedStudent);
            return updatedStudent;
        });

    public ValueTask DeleteStudentAsync(int studentId) =>
        TryCatch(operation: async () =>
        {
            Validate(inputs: studentId);
            Student? updatedStudent = studentService.GetStudent(studentId: studentId);

            if (updatedStudent != null)
            {
                await eventService.RaiseDeleteEventAsync(entityName: "updatedStudent", entity: updatedStudent);
                await studentService.DeleteStudentAsync(studentId: studentId);
            }
        });

    private static Student WithoutRelationships(Student student)
    {
        return new Student
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            SchoolId = student.SchoolId,
        };
    }
}