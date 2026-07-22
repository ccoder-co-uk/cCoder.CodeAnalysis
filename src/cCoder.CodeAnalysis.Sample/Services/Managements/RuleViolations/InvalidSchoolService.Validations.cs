// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Services.Processings.Validations;

namespace cCoder.CodeAnalysis.Sample.Services.Managements.RuleViolations;

internal sealed partial class InvalidSchoolService
{
private static void Validate(params object?[] inputs)
	{
		ValidationRulesEngine.Validate(inputs:inputs);
	}
}