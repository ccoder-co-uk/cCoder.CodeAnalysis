// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Processings.Teachers;

internal interface ITeacherProcessingService
{
    ValueTask AddOrUpdateTeachersAsync(IEnumerable<Teacher> teachers, int schoolId);

    ValueTask DeleteTeachersAsync(IEnumerable<Teacher> deletedTeachers);
}