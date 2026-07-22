// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Orchestrations.RuleViolations;

internal interface IInvalidOrchestrationService
{
	ValueTask ImportSchoolAsync(School school);
}