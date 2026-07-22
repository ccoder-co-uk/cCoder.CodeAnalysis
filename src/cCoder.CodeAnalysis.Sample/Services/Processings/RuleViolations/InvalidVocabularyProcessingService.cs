// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations;

internal sealed partial class InvalidVocabularyProcessingService : IInvalidVocabularyProcessingService
{
	public Student InsertStudent(Student newStudent)
=>
	    TryCatch(operation:() => {
			Validate(inputs:[newStudent]);
			return newStudent;
		});}