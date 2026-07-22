// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;
using cCoder.CodeAnalysis.Sample.Services.Processings.Students;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Processings.Students;

public sealed partial class StudentProcessingServiceTests
{
    [Fact]
    public async Task AddOrUpdateStudentsAsyncShouldExerciseBothPersistenceBranches()
    {
        // Given
        // When
        // Then
        Student[] items = CreateStudents();

        studentServiceMock
            .Setup(expression:(IStudentService studentService) => studentService.AddStudentAsync(newStudent:items[0]))
            .Returns(valueFunction:() => ValueTask.FromResult(result:items[0]));

        studentServiceMock
            .Setup(expression:(IStudentService studentService) => studentService.UpdateStudentAsync(updatedStudent:items[1]))
            .Returns(valueFunction:() => ValueTask.FromResult(result:items[1]));

        StudentProcessingService service = CreateStudentProcessingService();
        await service.AddOrUpdateStudentsAsync(students:items, schoolId:11);
        studentServiceMock.Verify(expression:(IStudentService foundation) => foundation.AddStudentAsync(newStudent:items[0]), times:Times.Once);
        studentServiceMock.Verify(expression:(IStudentService foundation) => foundation.UpdateStudentAsync(updatedStudent:items[1]), times:Times.Once);
    }
}