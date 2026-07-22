// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Teachers;
using FluentAssertions;

namespace cCoder.CodeAnalysis.Sample.Tests.Services.Foundations.Teachers;

public sealed partial class TeacherServiceTests
{
    [Fact]
    public void GetTeacherShouldReturnMatchingTeacher()
    {
        // Given
        // When
        // Then
        Teacher expectedTeacher = CreateTeacher();

        teacherBrokerMock
            .Setup(expression:(ITeacherBroker broker) => broker.SelectAllTeachers())
            .Returns(value:CreateTeachers(teacher:expectedTeacher));

        TeacherService service = CreateTeacherService();
        Teacher actualTeacher = service.GetTeacher(teacherId:expectedTeacher.Id)!;

        ((object)actualTeacher).Should()
            .BeSameAs(expected:expectedTeacher, because:"");
    }
}