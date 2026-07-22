// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Coordinations.SchoolImports;

internal interface ISchoolImportValidationCoordinationService
{
    bool CanImportSchool(School school);
}