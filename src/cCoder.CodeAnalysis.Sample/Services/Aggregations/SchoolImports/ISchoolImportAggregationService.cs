// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Aggregations.SchoolImports;

internal interface ISchoolImportAggregationService
{
    ValueTask ImportSchoolAsync(School school);
}