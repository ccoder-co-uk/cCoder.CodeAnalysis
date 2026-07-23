// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.Schools;

internal sealed partial class SchoolService(ISchoolBroker schoolBroker) : ISchoolService
{
    public School? GetSchool(int schoolId) =>
        TryCatch(operation: () =>
        {
            ValidateSchoolOnGet(schoolId: schoolId);

            return schoolBroker.SelectAllSchools()
                .FirstOrDefault(predicate: (School item) => item.Id == schoolId);
        });

    public IQueryable<School> GetSchools()
    {
        return TryCatch(operation: () => schoolBroker.SelectAllSchools());
    }

    public ValueTask<School> AddSchoolAsync(School newSchool) =>
        TryCatch<School>(operation: async () =>
        {
            ValidateSchoolOnAdd(newSchool: newSchool);
            School storageSchool = WithoutRelationships(school: newSchool);
            await schoolBroker.InsertSchoolAsync(newSchool: storageSchool);
            return storageSchool;
        });

    public ValueTask<School> UpdateSchoolAsync(School updatedSchool) =>
        TryCatch<School>(operation: async () =>
        {
            ValidateSchoolOnUpdate(updatedSchool: updatedSchool);
            School storageSchool = WithoutRelationships(school: updatedSchool);
            await schoolBroker.UpdateSchoolAsync(updatedSchool: storageSchool);
            return storageSchool;
        });

    public ValueTask DeleteSchoolAsync(int schoolId) =>
        TryCatch(operation: async () =>
        {
            ValidateSchoolOnDelete(schoolId: schoolId);

            School? deletedSchool = schoolBroker
                .SelectAllSchools()
                .FirstOrDefault(predicate: (School item) => item.Id == schoolId);

            if (deletedSchool != null)
            {
                await schoolBroker.DeleteSchoolAsync(deletedSchool: deletedSchool);
            }
        });

    private static School WithoutRelationships(School school)
    {
        return new School { Id = school.Id, Name = school.Name };
    }
}