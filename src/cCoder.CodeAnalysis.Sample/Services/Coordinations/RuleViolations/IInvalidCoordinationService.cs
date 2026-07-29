// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Coordinations.RuleViolations;

internal interface IInvalidCoordinationService
{
    ValueTask ImportSchoolAsync(School school);
}