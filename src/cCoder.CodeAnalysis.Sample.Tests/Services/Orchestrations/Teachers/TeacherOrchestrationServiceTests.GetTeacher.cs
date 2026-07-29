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
    public void GetTeacherShouldReturnTeacherOnHappyPath()
    {
        // Given
        // When
        // Then
        Teacher expectedTeacher = CreateTeacher();

        teacherServiceMock
            .Setup(expression: (ITeacherService teacherService) => teacherService.GetTeacher(teacherId: 7))
            .Returns(value: expectedTeacher);

        TeacherOrchestrationService service = CreateTeacherOrchestrationService();
        Teacher actualTeacher = service.GetTeacher(teacherId: 7)!;

        ((object)actualTeacher).Should()
            .BeSameAs(expected: expectedTeacher, because: "");
    }
}