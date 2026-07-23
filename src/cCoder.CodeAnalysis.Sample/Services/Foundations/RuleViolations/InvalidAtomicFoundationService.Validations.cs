// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Processings.Validations;

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations;

internal sealed partial class InvalidAtomicFoundationService
{
private static void Validate(params object?[] inputs)
{
		ValidationRulesEngine.Validate(inputs:inputs);
}

private static void ValidateStudentOnCreate(Student newStudent)
{
	Validate(inputs:newStudent);
}

private static void ValidateStudentOnAdd(Student newStudent)
{
	if (newStudent is null)
	{
		throw new ArgumentNullException(nameof(newStudent));
	}
}
}