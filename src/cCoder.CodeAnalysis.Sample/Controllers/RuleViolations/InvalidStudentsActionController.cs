// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Students;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.CodeAnalysis.Sample.Controllers.RuleViolations;

[ApiController]
[Route("api/students-invalid-action")]
public sealed class InvalidStudentsActionController(IStudentOrchestrationService studentOrchestrationService) : ControllerBase
{
    [HttpGet]
    public ActionResult<IQueryable<Student>> RetrieveStudents()
    {
        return Ok(value: studentOrchestrationService.GetStudents());
    }
}