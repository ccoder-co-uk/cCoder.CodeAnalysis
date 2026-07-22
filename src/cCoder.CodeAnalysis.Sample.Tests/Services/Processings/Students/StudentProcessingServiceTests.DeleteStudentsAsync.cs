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
    public async Task DeleteStudentsAsyncShouldDeleteEveryItem()
    {
        // Given
        // When
        // Then
        Student[] items = CreateStudents();

        studentServiceMock
            .Setup(expression:(IStudentService studentService) => studentService.DeleteStudentAsync(studentId:It.IsAny<int>()))
            .Returns(value:ValueTask.CompletedTask);

        StudentProcessingService service = CreateStudentProcessingService();
        await service.DeleteStudentsAsync(deletedStudents:items);

        studentServiceMock.Verify(
expression:            (IStudentService foundation) => foundation.DeleteStudentAsync(studentId:It.IsAny<int>()),
times:            Times.Exactly(callCount:2)
        );
    }
}