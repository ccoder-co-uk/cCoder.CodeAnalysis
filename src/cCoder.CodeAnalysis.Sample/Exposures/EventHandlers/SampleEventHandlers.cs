// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.Sample.Exposures.EventHandlers;

internal sealed class SampleEventHandlers(
    IStudentEventHandlers studentEventHandlers,
    ITeacherEventHandlers teacherEventHandlers,
    ICourseEventHandlers courseEventHandlers) : ISampleEventHandlers
{
    public void ListenToAllEvents() =>
        this.ListenToAllEvents(
            studentEventHandlers: studentEventHandlers,
            teacherEventHandlers: teacherEventHandlers,
            courseEventHandlers: courseEventHandlers);
}