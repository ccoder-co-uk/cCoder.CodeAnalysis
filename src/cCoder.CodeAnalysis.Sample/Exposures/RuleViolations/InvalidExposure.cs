// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Storage;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Teachers;

namespace cCoder.CodeAnalysis.Sample.Exposures.RuleViolations;

internal sealed class InvalidExposure(IStudentService studentService, ITeacherService teacherService, IStudentBroker studentBroker)
{
	public string Value { get; set; } = string.Empty;

	public static SchoolContext CreateContext()
	{
		return null!;
	}

	internal void Execute(bool shouldExecute)
	{
		if (shouldExecute)
		{
			Value = (studentService.GetStudents()
			    .Count() + teacherService.GetTeachers()
			    .Count() + studentBroker.SelectAllStudents()
			    .Count()).ToString();
		}

		while (shouldExecute)
		{
			shouldExecute = false;
		}
	}

	internal void ExecuteSequence()
	{
		studentService.GetStudents();
		teacherService.GetTeachers();
		AllowAnyOrigin();
		AllowCredentials();
	}

	private static void AllowAnyOrigin() { }

	private static void AllowCredentials() { }
}