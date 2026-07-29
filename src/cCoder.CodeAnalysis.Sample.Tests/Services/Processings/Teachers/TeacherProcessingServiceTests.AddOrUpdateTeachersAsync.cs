// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Teachers;
using cCoder.CodeAnalysis.Sample.Services.Processings.Teachers;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Processings.Teachers;

public sealed partial class TeacherProcessingServiceTests
{
    [Fact]
    public async Task AddOrUpdateTeachersAsyncShouldExerciseBothPersistenceBranches()
    {
        // Given
        // When
        // Then
        Teacher[] items = CreateTeachers();

        teacherServiceMock
            .Setup(expression: (ITeacherService teacherService) => teacherService.AddTeacherAsync(newTeacher: items[0]))
            .Returns(valueFunction: () => ValueTask.FromResult(result: items[0]));

        teacherServiceMock
            .Setup(expression: (ITeacherService teacherService) => teacherService.UpdateTeacherAsync(updatedTeacher: items[1]))
            .Returns(valueFunction: () => ValueTask.FromResult(result: items[1]));

        TeacherProcessingService service = CreateTeacherProcessingService();
        await service.AddOrUpdateTeachersAsync(teachers: items, schoolId: 11);
        teacherServiceMock.Verify(expression: (ITeacherService foundation) => foundation.AddTeacherAsync(newTeacher: items[0]), times: Times.Once);
        teacherServiceMock.Verify(expression: (ITeacherService foundation) => foundation.UpdateTeacherAsync(updatedTeacher: items[1]), times: Times.Once);
    }
}