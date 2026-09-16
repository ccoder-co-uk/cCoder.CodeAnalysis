// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Brokers.Storage.RuleViolations;

internal sealed class InvalidStorageBroker(SchoolContext context) : IInvalidStorageBroker
{
    public IQueryable<Student> GetStudents()
=>
        context.Students;
}