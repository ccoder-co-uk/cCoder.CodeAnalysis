// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Aggregations.SchoolImports;

namespace cCoder.CodeAnalysis.Sample.Exposures.SchoolImports;

internal sealed class SchoolImportManager(ISchoolImportAggregationService importService) : ISchoolImportManager
{
    public ValueTask ImportSchoolAsync(School school)
    {
        return importService.ImportSchoolAsync(school: school);
    }
}