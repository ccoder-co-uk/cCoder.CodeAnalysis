// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Foundations.Students;

namespace cCoder.CodeAnalysis.Sample.Services.Foundations.RuleViolations;

public sealed class InvalidFoundationService : IInvalidFoundationService
{
	private readonly IStudentService studentService;

	internal InvalidFoundationService(IStudentService studentService)
	{
		this.studentService = studentService;
	}

	public void Execute()
	{
		Perform();
	}

	public void Perform()
	{
		for (int index = 0; index < 1; index++)
		{
			studentService.GetStudent(studentId:index);
		}
	}

	public Student ConvertTeacher(Teacher teacher)
	{
		return new Student
		{
			FirstName = teacher.FirstName,
			LastName = teacher.LastName
		};
	}
}