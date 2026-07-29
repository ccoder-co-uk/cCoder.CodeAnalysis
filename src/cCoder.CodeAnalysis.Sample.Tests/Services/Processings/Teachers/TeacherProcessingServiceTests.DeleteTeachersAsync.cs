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
    public async Task DeleteTeachersAsyncShouldDeleteEveryItem()
    {
        // Given
        // When
        // Then
        Teacher[] items = CreateTeachers();

        teacherServiceMock
            .Setup(expression: (ITeacherService teacherService) => teacherService.DeleteTeacherAsync(teacherId: It.IsAny<int>()))
            .Returns(value: ValueTask.CompletedTask);

        TeacherProcessingService service = CreateTeacherProcessingService();
        await service.DeleteTeachersAsync(deletedTeachers: items);

        teacherServiceMock.Verify(
expression: (ITeacherService foundation) => foundation.DeleteTeacherAsync(teacherId: It.IsAny<int>()),
times: Times.Exactly(callCount: 2)
        );
    }
}