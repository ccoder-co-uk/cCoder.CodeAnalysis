// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Processings.Schools;

internal interface ISchoolImportProcessingService
{
    ValueTask ImportSchoolAsync(School school);
}