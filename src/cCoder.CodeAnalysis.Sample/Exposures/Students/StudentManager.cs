// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Students;

namespace cCoder.CodeAnalysis.Sample.Exposures.Students;

internal sealed class StudentManager(IStudentOrchestrationService service) : IStudentManager
{
    public Student? GetStudent(int studentId)
    {
        return service.GetStudent(studentId: studentId);
    }

    public IQueryable<Student> GetStudents()
    {
        return service.GetStudents();
    }

    public ValueTask<Student> AddStudentAsync(Student newStudent)
    {
        return service.AddStudentAsync(newStudent: newStudent);
    }

    public ValueTask<Student> UpdateStudentAsync(Student updatedStudent)
    {
        return service.UpdateStudentAsync(updatedStudent: updatedStudent);
    }

    public ValueTask DeleteStudentAsync(int studentId)
    {
        return service.DeleteStudentAsync(studentId: studentId);
    }
}