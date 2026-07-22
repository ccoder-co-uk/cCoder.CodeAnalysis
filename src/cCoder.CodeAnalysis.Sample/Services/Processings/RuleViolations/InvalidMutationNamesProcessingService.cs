// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations;

internal sealed partial class InvalidMutationNamesProcessingService : IInvalidMutationNamesProcessingService
{
	public Student AddAsync(Student newStudent)
=>
	    TryCatch(operation:() => {
			Validate(inputs:[newStudent]);
			return newStudent;
		});

	public Student AddStudent(Student student)
=>
	    TryCatch(operation:() => {
			Validate(inputs:[student]);
			return student;
		});

	public Student UpdateStudent(Student student)
=>
	    TryCatch(operation:() => {
			Validate(inputs:[student]);
			return student;
		});

	public Student DeleteStudent(Student student)
=>
	    TryCatch(operation:() => {
			Validate(inputs:[student]);
			return student;
		});}