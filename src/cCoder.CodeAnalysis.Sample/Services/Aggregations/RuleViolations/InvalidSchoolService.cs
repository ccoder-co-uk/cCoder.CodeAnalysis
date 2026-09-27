// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Managements.SchoolImports;

namespace cCoder.CodeAnalysis.Sample.Services.Aggregations.RuleViolations;

internal sealed partial class InvalidSchoolService(ISchoolImportReadinessManagementService readinessService) : IInvalidSchoolService
{
    public bool CanAggregate()
=>
        TryCatch(operation: () =>
        {
            return readinessService.CanImportSchool(school: new School());
        });
}