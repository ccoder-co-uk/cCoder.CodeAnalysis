// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Exposures.SchoolImports;

public interface ISchoolImportManager
{
    ValueTask ImportSchoolAsync(School school);
}