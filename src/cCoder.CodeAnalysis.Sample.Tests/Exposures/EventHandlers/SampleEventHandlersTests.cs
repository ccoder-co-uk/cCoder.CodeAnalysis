// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Exposures.EventHandlers;
using Moq;

namespace cCoder.CodeAnalysis.Sample.Tests.Exposures.EventHandlers;

public sealed partial class SampleEventHandlersTests
{
    [Fact]
    public void ListenToAllEventsInvokesEachEntityRegistrationInOrder()
    {
        // Given
        MockSequence sequence = new();
        Mock<IStudentEventHandlers> studentEventHandlersMock = new();
        Mock<ITeacherEventHandlers> teacherEventHandlersMock = new();
        Mock<ICourseEventHandlers> courseEventHandlersMock = new();

        studentEventHandlersMock.InSequence(sequence: sequence)
            .Setup(expression: eventHandlers => eventHandlers.ListenToStudentEvents());

        teacherEventHandlersMock.InSequence(sequence: sequence)
            .Setup(expression: eventHandlers => eventHandlers.ListenToTeacherEvents());

        courseEventHandlersMock.InSequence(sequence: sequence)
            .Setup(expression: eventHandlers => eventHandlers.ListenToCourseEvents());

        SampleEventHandlers eventHandlers = new(
            studentEventHandlers: studentEventHandlersMock.Object,
            teacherEventHandlers: teacherEventHandlersMock.Object,
            courseEventHandlers: courseEventHandlersMock.Object);

        // When
        eventHandlers.ListenToAllEvents();

        // Then
        studentEventHandlersMock.VerifyAll();
        teacherEventHandlersMock.VerifyAll();
        courseEventHandlersMock.VerifyAll();
    }
}