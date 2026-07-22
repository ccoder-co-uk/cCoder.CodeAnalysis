// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Teachers;

namespace cCoder.CodeAnalysis.Sample.Services.Orchestrations.RuleViolations;

internal sealed partial class InvalidSchoolService(IStudentService studentService, ITeacherService teacherService) : IInvalidSchoolService
{
	public int CountPeople()
=>
	    TryCatch(operation:() => {
			int num = studentService.GetStudents()
			    .Count();

			int num2 = teacherService.GetTeachers()
			    .Count();

			return num + num2;
		});}