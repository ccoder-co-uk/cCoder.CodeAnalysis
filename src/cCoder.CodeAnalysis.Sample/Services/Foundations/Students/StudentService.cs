// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.Students;

internal sealed partial class StudentService(IStudentBroker studentBroker) : IStudentService
{
    public Student? GetStudent(int studentId) =>
        TryCatch(operation: () =>
        {
            ValidateStudentOnGet(studentId: studentId);

            return studentBroker.SelectAllStudents()
                .FirstOrDefault(predicate: (Student item) => item.Id == studentId);
        });

    public IQueryable<Student> GetStudents()
    {
        return TryCatch(operation: () => studentBroker.SelectAllStudents());
    }

    public ValueTask<Student> AddStudentAsync(Student newStudent) =>
        TryCatch<Student>(operation: async () =>
        {
            ValidateStudentOnAdd(newStudent: newStudent);
            Student storageStudent = WithoutRelationships(student: newStudent);
            await studentBroker.InsertStudentAsync(newStudent: storageStudent);
            return storageStudent;
        });

    public ValueTask<Student> UpdateStudentAsync(Student updatedStudent) =>
        TryCatch<Student>(operation: async () =>
        {
            ValidateStudentOnUpdate(updatedStudent: updatedStudent);
            Student storageStudent = WithoutRelationships(student: updatedStudent);
            await studentBroker.UpdateStudentAsync(updatedStudent: storageStudent);
            return storageStudent;
        });

    public ValueTask DeleteStudentAsync(int studentId) =>
        TryCatch(operation: async () =>
        {
            ValidateStudentOnDelete(studentId: studentId);

            Student? deletedStudent = studentBroker
                .SelectAllStudents()
                .FirstOrDefault(predicate: (Student item) => item.Id == studentId);

            if (deletedStudent != null)
            {
                await studentBroker.DeleteStudentAsync(deletedStudent: deletedStudent);
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
            Courses = [],
        };
    }
}