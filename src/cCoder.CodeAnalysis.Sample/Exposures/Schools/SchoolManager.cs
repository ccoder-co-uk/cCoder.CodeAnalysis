// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Schools;

namespace cCoder.CodeAnalysis.Sample.Exposures.Schools;

internal sealed class SchoolManager(ISchoolOrchestrationService service) : ISchoolManager
{
    public School? GetSchool(int schoolId)
    {
        return service.GetSchool(schoolId: schoolId);
    }

    public IQueryable<School> GetSchools()
    {
        return service.GetSchools();
    }

    public ValueTask<School> AddSchoolAsync(School newSchool)
    {
        return service.AddSchoolAsync(newSchool: newSchool);
    }

    public ValueTask<School> UpdateSchoolAsync(School updatedSchool)
    {
        return service.UpdateSchoolAsync(updatedSchool: updatedSchool);
    }

    public ValueTask DeleteSchoolAsync(int schoolId)
    {
        return service.DeleteSchoolAsync(schoolId: schoolId);
    }
}