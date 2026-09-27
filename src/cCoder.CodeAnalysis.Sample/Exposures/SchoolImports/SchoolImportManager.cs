// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Aggregations.SchoolImports;
using cCoder.CodeAnalysis.Exposures;

namespace cCoder.CodeAnalysis.Sample.Exposures.SchoolImports;

internal sealed class SchoolImportManager(ISchoolImportAggregationService importService)
    : ISchoolImportManager, ICompositionExposure
{
    public ValueTask ImportSchoolAsync(School school)
    {
        return importService.ImportSchoolAsync(school: school);
    }
}