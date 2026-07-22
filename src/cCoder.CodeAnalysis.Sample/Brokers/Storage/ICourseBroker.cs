// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Brokers.Storage;

internal interface ICourseBroker
{
    IQueryable<Course> SelectAllCourses();

    ValueTask<Course> InsertCourseAsync(Course newCourse);

    ValueTask<Course> UpdateCourseAsync(Course updatedCourse);

    ValueTask<int> DeleteCourseAsync(Course deletedCourse);
}