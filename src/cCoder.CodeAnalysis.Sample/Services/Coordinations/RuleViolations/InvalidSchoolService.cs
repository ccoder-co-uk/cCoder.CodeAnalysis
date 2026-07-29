// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Services.Orchestrations.SchoolImports;

namespace cCoder.CodeAnalysis.Sample.Services.Coordinations.RuleViolations;

internal sealed partial class InvalidSchoolService(ISchoolStructureImportOrchestrationService structureService, ISchoolPeopleImportOrchestrationService peopleService) : IInvalidSchoolService
{
    public bool CanImportSchool()
=>
        TryCatch(operation: () =>
        {
            bool flag = structureService != null;
            bool flag2 = peopleService != null;
            return flag && flag2;
        });
}