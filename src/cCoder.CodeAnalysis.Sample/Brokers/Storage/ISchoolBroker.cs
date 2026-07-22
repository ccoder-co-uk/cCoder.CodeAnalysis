// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Brokers.Storage;

internal interface ISchoolBroker
{
    IQueryable<School> SelectAllSchools();

    ValueTask<School> InsertSchoolAsync(School newSchool);

    ValueTask<School> UpdateSchoolAsync(School updatedSchool);

    ValueTask<int> DeleteSchoolAsync(School deletedSchool);
}