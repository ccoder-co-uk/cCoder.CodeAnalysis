// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Orchestrations.Schools;

internal interface ISchoolOrchestrationService
{
    School? GetSchool(int schoolId);

    IQueryable<School> GetSchools();

    ValueTask<School> AddSchoolAsync(School newSchool);

    ValueTask<School> UpdateSchoolAsync(School updatedSchool);

    ValueTask DeleteSchoolAsync(int schoolId);
}