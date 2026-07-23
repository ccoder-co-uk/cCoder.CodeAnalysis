// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.Courses;

internal sealed partial class CourseService(ICourseBroker courseBroker) : ICourseService
{
    public Course? GetCourse(int courseId) =>
        TryCatch(operation: () =>
        {
            ValidateCourseOnGet(courseId: courseId);

            return courseBroker.SelectAllCourses()
                .FirstOrDefault(predicate: (Course item) => item.Id == courseId);
        });

    public IQueryable<Course> GetCourses()
    {
        return TryCatch(operation: () => courseBroker.SelectAllCourses());
    }

    public ValueTask<Course> AddCourseAsync(Course newCourse) =>
        TryCatch<Course>(operation: async () =>
        {
            ValidateCourseOnAdd(newCourse: newCourse);
            Course storageCourse = WithoutRelationships(course: newCourse);
            await courseBroker.InsertCourseAsync(newCourse: storageCourse);
            return storageCourse;
        });

    public ValueTask<Course> UpdateCourseAsync(Course updatedCourse) =>
        TryCatch<Course>(operation: async () =>
        {
            ValidateCourseOnUpdate(updatedCourse: updatedCourse);
            Course storageCourse = WithoutRelationships(course: updatedCourse);
            await courseBroker.UpdateCourseAsync(updatedCourse: storageCourse);
            return storageCourse;
        });

    public ValueTask DeleteCourseAsync(int courseId) =>
        TryCatch(operation: async () =>
        {
            ValidateCourseOnDelete(courseId: courseId);

            Course? deletedCourse = courseBroker
                .SelectAllCourses()
                .FirstOrDefault(predicate: (Course item) => item.Id == courseId);

            if (deletedCourse != null)
            {
                await courseBroker.DeleteCourseAsync(deletedCourse: deletedCourse);
            }
        });

    private static Course WithoutRelationships(Course course)
    {
        return new Course
        {
            Id = course.Id,
            Name = course.Name,
            SchoolId = course.SchoolId,
            TeacherId = course.TeacherId,
        };
    }
}