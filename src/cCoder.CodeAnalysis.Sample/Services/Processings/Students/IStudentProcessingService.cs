// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Processings.Students;

internal interface IStudentProcessingService
{
    ValueTask AddOrUpdateStudentsAsync(IEnumerable<Student> students, int schoolId);

    ValueTask DeleteStudentsAsync(IEnumerable<Student> deletedStudents);
}