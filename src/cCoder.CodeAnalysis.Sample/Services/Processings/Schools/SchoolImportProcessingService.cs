// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Processings.Schools;

internal sealed partial class SchoolImportProcessingService(ISchoolService schoolService)
    : ISchoolImportProcessingService
{
    public ValueTask ImportSchoolAsync(School school) =>
        TryCatch(operation: async () =>
        {
            Validate(inputs: school);
            School schoolRecord = new School { Id = school.Id, Name = school.Name };

            if (schoolRecord.Id == 0)
            {
                School addedSchool = await schoolService.AddSchoolAsync(newSchool: schoolRecord);
                school.Id = addedSchool.Id;
            }
            else
            {
                await schoolService.UpdateSchoolAsync(updatedSchool: schoolRecord);
            }
        });
}