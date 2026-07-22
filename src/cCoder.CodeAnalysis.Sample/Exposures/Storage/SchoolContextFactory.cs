// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using Microsoft.EntityFrameworkCore;

namespace cCoder.CodeAnalysis.Sample.Exposures.Storage;

internal sealed class SchoolContextFactory(IDbContextFactory<SchoolContext> contextFactory) : ISchoolContextFactory
{
    public SchoolContext CreateSchoolContext()
    {
        return contextFactory.CreateDbContext();
    }
}