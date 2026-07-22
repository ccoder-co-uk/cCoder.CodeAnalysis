// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Managements.SchoolImports;

namespace cCoder.CodeAnalysis.Sample.Services.Aggregations.SchoolImports;

internal sealed partial class SchoolImportAggregationService(
    ISchoolImportManagementService importService,
    ISchoolImportReadinessManagementService readinessService
) : ISchoolImportAggregationService
{
    public ValueTask ImportSchoolAsync(School school) =>
        TryCatch(operation: async () =>
        {
            Validate(inputs: school);

            if (readinessService.CanImportSchool(school: school))
            {
                await importService.ImportSchoolAsync(school: school);
            }
        });
}