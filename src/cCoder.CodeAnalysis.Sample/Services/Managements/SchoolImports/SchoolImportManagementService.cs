// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Coordinations.SchoolImports;

namespace cCoder.CodeAnalysis.Sample.Services.Managements.SchoolImports;

internal sealed partial class SchoolImportManagementService(
    ISchoolImportCoordinationService importService,
    ISchoolImportValidationCoordinationService validationService
) : ISchoolImportManagementService
{
    public ValueTask ImportSchoolAsync(School school) =>
        TryCatch(operation: async () =>
        {
            Validate(inputs: school);

            if (validationService.CanImportSchool(school: school))
            {
                await importService.ImportSchoolAsync(school: school);
            }
        });
}