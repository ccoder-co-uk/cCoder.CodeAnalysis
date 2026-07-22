// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Brokers.Storage.RuleViolations;

internal sealed class InvalidStorageBroker : IInvalidStorageBroker
{
	public IQueryable<Student> GetStudents()
=>
	    Array.Empty<Student>()
		    .AsQueryable();}