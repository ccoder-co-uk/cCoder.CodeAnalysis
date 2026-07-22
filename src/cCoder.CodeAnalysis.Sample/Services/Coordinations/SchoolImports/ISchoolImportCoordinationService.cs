// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Coordinations.SchoolImports;

internal interface ISchoolImportCoordinationService
{
    bool CanImportSchool(School school);

    ValueTask ImportSchoolAsync(School school);
}