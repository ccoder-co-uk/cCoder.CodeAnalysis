// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations;

internal interface IInvalidProcessingDependencyService
{
	ValueTask ImportSchoolAsync(School school);
}