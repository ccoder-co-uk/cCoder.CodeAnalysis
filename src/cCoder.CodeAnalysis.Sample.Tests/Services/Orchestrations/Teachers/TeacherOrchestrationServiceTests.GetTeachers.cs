// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Teachers;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Teachers;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.Teachers;

public sealed partial class TeacherOrchestrationServiceTests
{
    [Fact]
    public void GetTeachersShouldReturnTeachersOnHappyPath()
    {
        // Given
        // When
        // Then
        IQueryable<Teacher> expectedTeachers = CreateTeachers(teacher: CreateTeacher());

        teacherServiceMock
            .Setup(expression: (ITeacherService teacherService) => teacherService.GetTeachers())
            .Returns(value: expectedTeachers);

        TeacherOrchestrationService service = CreateTeacherOrchestrationService();
        IQueryable<Teacher> actualTeachers = service.GetTeachers();

        ((IEnumerable<Teacher>)actualTeachers).Should()
            .BeSameAs(expected: expectedTeachers, because: "");
    }
}