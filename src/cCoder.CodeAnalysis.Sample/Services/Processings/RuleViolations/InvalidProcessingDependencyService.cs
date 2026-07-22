// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;

namespace cCoder.CodeAnalysis.Sample.Services.Processings.RuleViolations;

internal sealed partial class InvalidProcessingDependencyService(ISchoolService schoolService, IStudentService studentService, IStudentService duplicateStudentService) : IInvalidProcessingDependencyService
{
	public async ValueTask ImportSchoolAsync(School school)
=>
	    await TryCatch(operation:async () => {
			Validate(inputs:[school]);

			await schoolService.AddSchoolAsync(newSchool:new School
			{
				Id = school.Id,
				Name = school.Name
			});

			_ = studentService.GetStudents()
			    .Count();

			_ = duplicateStudentService.GetStudents()
			    .Count();
		});}