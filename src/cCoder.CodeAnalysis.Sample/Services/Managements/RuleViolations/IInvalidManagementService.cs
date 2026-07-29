// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Managements.RuleViolations;

internal interface IInvalidManagementService
{
    ValueTask ImportSchoolAsync(School school);
}