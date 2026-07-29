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
        return Ok(value: studentManager.GetStudents());
    }

    [HttpGet("teachers")]
    public ActionResult<IQueryable<Teacher>> GetTeachers()
    {
        return Ok(value: teacherManager.GetTeachers());
    }
}