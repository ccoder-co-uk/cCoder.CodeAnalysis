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
    public void GetTeachersShouldReturnBrokerQuery()
    {
        // Given
        // When
        // Then
        IQueryable<Teacher> expectedTeachers = CreateTeachers();

        teacherBrokerMock.Setup(expression: (ITeacherBroker broker) => broker.SelectAllTeachers())
            .Returns(value: expectedTeachers);

        TeacherService service = CreateTeacherService();
        IQueryable<Teacher> actualTeachers = service.GetTeachers();

        ((IEnumerable<Teacher>)actualTeachers).Should()
            .BeSameAs(expected: expectedTeachers, because: "");
    }
}