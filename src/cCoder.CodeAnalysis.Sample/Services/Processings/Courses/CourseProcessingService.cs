// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Courses;

namespace cCoder.CodeAnalysis.Sample.Services.Processings.Courses;

internal sealed partial class CourseProcessingService(ICourseService courseService) : ICourseProcessingService
{
    public ValueTask AddOrUpdateCoursesAsync(IEnumerable<Course> courses, int schoolId, int? teacherId = null) =>
        TryCatch(operation: async () =>
        {
            Validate(inputs: [courses, schoolId]);

            foreach (Course course in courses)
            {
                course.SchoolId = schoolId;
                course.TeacherId = teacherId ?? course.TeacherId;

                if (course.Id == 0)
                {
                    await courseService.AddCourseAsync(newCourse: course);
                }
                else
                {
                    await courseService.UpdateCourseAsync(updatedCourse: course);
                }
            }
        });

    public ValueTask DeleteCoursesAsync(IEnumerable<Course> deletedCourses) =>
        TryCatch(operation: async () =>
        {
            Validate(inputs: deletedCourses);

            foreach (Course deletedCourse in deletedCourses)
            {
                await courseService.DeleteCourseAsync(courseId: deletedCourse.Id);
            }
        });
}