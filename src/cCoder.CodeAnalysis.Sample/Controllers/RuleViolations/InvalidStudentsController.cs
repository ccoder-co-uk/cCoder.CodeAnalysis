// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Exposures.Students;
using cCoder.CodeAnalysis.Sample.Exposures.Teachers;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.CodeAnalysis.Sample.Controllers.RuleViolations;

[ApiController]
[Route("api/[controller]")]
public sealed class InvalidStudentsController(IStudentManager studentManager, ITeacherManager teacherManager) : ControllerBase
{
    [HttpGet("students")]
    public ActionResult<IQueryable<Student>> GetStudents()
    {
        try
        {
            return Ok(value: studentManager.GetStudents());
        }
        catch (Exception)
        {
            return StatusCode(statusCode: 500);
        }
    }

    [HttpGet("teachers")]
    public ActionResult<IQueryable<Teacher>> GetTeachers()
    {
        try
        {
            return Ok(value: teacherManager.GetTeachers());
        }
        catch (Exception)
        {
            return StatusCode(statusCode: 500);
        }
    }
}