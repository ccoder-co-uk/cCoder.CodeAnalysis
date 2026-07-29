// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Schools;
using cCoder.CodeAnalysis.Sample.Services.Processings.Schools;
using FluentAssertions;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Processings.Schools;

public sealed partial class SchoolImportProcessingServiceTests
{
    [Fact]
    public async Task ImportSchoolAsyncExercisesAddAndUpdateBranches()
    {
        // Given
        // When
        // Then
        School newSchool = CreateSchool();
        School existingSchool = CreateSchool(schoolId: 7);
        School savedSchool = CreateSchool(schoolId: 11);

        schoolServiceMock
            .Setup(expression: (ISchoolService schoolService) => schoolService.AddSchoolAsync(newSchool: It.IsAny<School>()))
            .Returns(valueFunction: () => ValueTask.FromResult(result: savedSchool));

        SchoolImportProcessingService service = CreateSchoolImportProcessingService();
        await service.ImportSchoolAsync(school: newSchool);
        await service.ImportSchoolAsync(school: existingSchool);

        newSchool.Id.Should()
            .Be(expected: savedSchool.Id, because: "");

        schoolServiceMock.Verify(
expression: (ISchoolService foundation) => foundation.UpdateSchoolAsync(updatedSchool: It.Is(match: (School school) => school.Id == 7)),
times: Times.Once
        );
    }
}