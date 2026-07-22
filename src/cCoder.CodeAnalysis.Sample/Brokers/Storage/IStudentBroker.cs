// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Brokers.Storage;

internal interface IStudentBroker
{
    IQueryable<Student> SelectAllStudents();

    ValueTask<Student> InsertStudentAsync(Student newStudent);

    ValueTask<Student> UpdateStudentAsync(Student updatedStudent);

    ValueTask<int> DeleteStudentAsync(Student deletedStudent);
}