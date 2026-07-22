// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.Students;

internal interface IStudentService
{
    Student? GetStudent(int studentId);

    IQueryable<Student> GetStudents();

    ValueTask<Student> AddStudentAsync(Student newStudent);

    ValueTask<Student> UpdateStudentAsync(Student updatedStudent);

    ValueTask DeleteStudentAsync(int studentId);
}