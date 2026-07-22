// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.Courses;

internal interface ICourseService
{
    Course? GetCourse(int courseId);

    IQueryable<Course> GetCourses();

    ValueTask<Course> AddCourseAsync(Course newCourse);

    ValueTask<Course> UpdateCourseAsync(Course updatedCourse);

    ValueTask DeleteCourseAsync(int courseId);
}