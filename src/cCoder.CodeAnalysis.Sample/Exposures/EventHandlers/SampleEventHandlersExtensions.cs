// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Exposures.EventHandlers;

internal static class SampleEventHandlersExtensions
{
    internal static void ListenToAllEvents(
        this ISampleEventHandlers sampleEventHandlers,
        IStudentEventHandlers studentEventHandlers,
        ITeacherEventHandlers teacherEventHandlers,
        ICourseEventHandlers courseEventHandlers)
    {
        studentEventHandlers.ListenToStudentEvents();
        teacherEventHandlers.ListenToTeacherEvents();
        courseEventHandlers.ListenToCourseEvents();
    }
}