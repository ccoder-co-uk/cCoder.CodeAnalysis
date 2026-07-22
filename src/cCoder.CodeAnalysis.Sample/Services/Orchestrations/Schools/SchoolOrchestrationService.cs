// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Events;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Orchestrations.Schools;

internal sealed partial class SchoolOrchestrationService(ISchoolService schoolService, IEntityEventService eventService)
    : ISchoolOrchestrationService
{
    public School? GetSchool(int schoolId) =>
        TryCatch(operation: () =>
        {
            Validate(inputs: schoolId);
            return schoolService.GetSchool(schoolId: schoolId);
        });

    public IQueryable<School> GetSchools()
    {
        return TryCatch(operation: () => schoolService.GetSchools());
    }

    public ValueTask<School> AddSchoolAsync(School newSchool) =>
        TryCatch<School>(operation: async () =>
        {
            Validate(inputs: newSchool);
            School result = await schoolService.AddSchoolAsync(newSchool: WithoutRelationships(school: newSchool));
            newSchool.Id = result.Id;
            await eventService.RaiseAddEventAsync(entityName: "newSchool", entity: newSchool);
            return newSchool;
        });

    public ValueTask<School> UpdateSchoolAsync(School updatedSchool) =>
        TryCatch<School>(operation: async () =>
        {
            Validate(inputs: updatedSchool);
            await schoolService.UpdateSchoolAsync(updatedSchool: WithoutRelationships(school: updatedSchool));
            await eventService.RaiseUpdateEventAsync(entityName: "updatedSchool", entity: updatedSchool);
            return updatedSchool;
        });

    public ValueTask DeleteSchoolAsync(int schoolId) =>
        TryCatch(operation: async () =>
        {
            Validate(inputs: schoolId);
            School? updatedSchool = schoolService.GetSchool(schoolId: schoolId);

            if (updatedSchool != null)
            {
                await eventService.RaiseDeleteEventAsync(entityName: "updatedSchool", entity: updatedSchool);
                await schoolService.DeleteSchoolAsync(schoolId: schoolId);
            }
        });

    private static School WithoutRelationships(School school)
    {
        return new School { Id = school.Id, Name = school.Name };
    }
}