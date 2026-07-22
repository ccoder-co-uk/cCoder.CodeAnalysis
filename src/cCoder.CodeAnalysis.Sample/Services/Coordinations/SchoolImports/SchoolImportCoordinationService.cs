// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports;

namespace cCoder.CodeAnalysis.Sample.Services.Coordinations.SchoolImports;

internal sealed partial class SchoolImportCoordinationService(
    ISchoolStructureImportOrchestrationService structureService,
    ISchoolPeopleImportOrchestrationService peopleService
) : ISchoolImportCoordinationService
{
    public bool CanImportSchool(School school) =>
        TryCatch(operation: () =>
        {
            Validate(inputs: school);
            return structureService.CanImportSchool(school: school) && peopleService.CanImportSchool(school: school);
        });

    public ValueTask ImportSchoolAsync(School school) =>
        TryCatch(operation: async () =>
        {
            Validate(inputs: school);
            await structureService.ImportSchoolAsync(school: school);
            await peopleService.ImportSchoolAsync(school: school);
        });
}