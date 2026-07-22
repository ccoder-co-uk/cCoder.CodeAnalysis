// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports;

namespace cCoder.CodeAnalysis.Sample.Services.Coordinations.SchoolImports;

internal sealed partial class SchoolImportValidationCoordinationService(
    ISchoolStructureImportOrchestrationService structureService,
    ISchoolPeopleImportOrchestrationService peopleService
) : ISchoolImportValidationCoordinationService
{
    public bool CanImportSchool(School school) =>
        TryCatch(operation: () =>
        {
            Validate(inputs: school);
            return structureService.CanImportSchool(school: school) && peopleService.CanImportSchool(school: school);
        });
}