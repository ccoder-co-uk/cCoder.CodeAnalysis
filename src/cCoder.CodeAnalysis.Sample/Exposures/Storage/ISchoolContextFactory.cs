// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;

namespace cCoder.CodeAnalysis.Sample.Exposures.Storage;

internal interface ISchoolContextFactory
{
    SchoolContext CreateSchoolContext();
}