// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Processings.Courses;

internal interface ICourseProcessingService
{
    ValueTask AddOrUpdateCoursesAsync(IEnumerable<Course> courses, int schoolId, int? teacherId = null);

    ValueTask DeleteCoursesAsync(IEnumerable<Course> deletedCourses);
}