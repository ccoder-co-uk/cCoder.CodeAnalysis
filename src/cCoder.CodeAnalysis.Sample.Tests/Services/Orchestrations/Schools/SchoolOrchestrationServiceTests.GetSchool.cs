// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Schools;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Orchestrations.Schools;

public sealed partial class SchoolOrchestrationServiceTests
{
    [Fact]
    public void GetSchoolShouldReturnSchoolOnHappyPath()
    {
        // Given
        // When
        // Then
        School expectedSchool = new School { Id = 7 };

        schoolServiceMock.Setup(expression: (ISchoolService schoolService) => schoolService.GetSchool(schoolId: 7))
            .Returns(value: expectedSchool);

        SchoolOrchestrationService service = CreateSchoolOrchestrationService();
        School actualSchool = service.GetSchool(schoolId: 7)!;

        ((object)actualSchool).Should()
            .BeSameAs(expected: expectedSchool, because: "");
    }
}