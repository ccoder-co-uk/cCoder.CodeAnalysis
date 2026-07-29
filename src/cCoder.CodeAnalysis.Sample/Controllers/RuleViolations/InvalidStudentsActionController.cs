// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Exposures.Students;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.CodeAnalysis.Sample.Controllers.RuleViolations;

[ApiController]
[Route("api/students-invalid-action")]
public sealed class InvalidStudentsActionController(IStudentManager studentManager) : ControllerBase
{
    [HttpGet]
    public ActionResult<IQueryable<Student>> RetrieveStudents()
    {
        return Ok(value: studentManager.GetStudents());
    }
}