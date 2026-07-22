// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports;

internal interface ISchoolStructureImportOrchestrationService
{
    bool CanImportSchool(School school);

    ValueTask ImportSchoolAsync(School school);
}