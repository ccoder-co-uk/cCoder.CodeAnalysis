// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Processings.Courses;

namespace cCoder.CodeAnalysis.Sample.Services.Orchestrations.RuleViolations;

internal sealed partial class InvalidOrchestrationService(ICourseProcessingService courseProcessingService) : IInvalidOrchestrationService
{
	public ValueTask ImportSchoolAsync(School school)
=>
	    TryCatch(operation:() => {
			Validate(inputs:[school]);
			return courseProcessingService.AddOrUpdateCoursesAsync(courses:school.Courses.ToArray(), schoolId:school.Id);
		});}