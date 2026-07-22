// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Coordinations.SchoolImports;

namespace cCoder.CodeAnalysis.Sample.Services.Managements.SchoolImports;

internal sealed partial class SchoolImportReadinessManagementService(
    ISchoolImportCoordinationService importService,
    ISchoolImportValidationCoordinationService validationService
) : ISchoolImportReadinessManagementService
{
    public bool CanImportSchool(School school) =>
        TryCatch(operation: () =>
        {
            Validate(inputs: school);
            return validationService.CanImportSchool(school: school) && importService.CanImportSchool(school: school);
        });
}