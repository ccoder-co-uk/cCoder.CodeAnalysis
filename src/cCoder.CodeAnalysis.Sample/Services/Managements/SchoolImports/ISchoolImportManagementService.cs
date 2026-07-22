// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Managements.SchoolImports;

internal interface ISchoolImportManagementService
{
    ValueTask ImportSchoolAsync(School school);
}