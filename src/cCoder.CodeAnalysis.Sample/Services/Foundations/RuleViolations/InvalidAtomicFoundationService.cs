// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations;

internal sealed partial class InvalidAtomicFoundationService(IStudentBroker studentBroker) : IInvalidAtomicFoundationService
{
    public ValueTask<Student> AddStudentAsync(Student newStudent)
=>
        TryCatch(operation: () =>
        {
            Validate(inputs: [newStudent]);
            _ = newStudent.Id;
            return studentBroker.InsertStudentAsync(newStudent: newStudent);
        });
}