// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Aggregations.RuleViolations;

internal interface IInvalidAggregationService
{
	ValueTask ImportSchoolAsync(School school);
}