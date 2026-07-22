// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Exposures.Students;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.CodeAnalysis.Sample.Controllers.RuleViolations;

[ApiController]
[Route("api/students-invalid-name")]
public sealed class InvalidStudentsEndpoint(IStudentManager studentManager) : ControllerBase
{
	[HttpGet]
	public ActionResult<IQueryable<Student>> Get()
	{
		return Ok(value:studentManager.GetStudents());
	}
}