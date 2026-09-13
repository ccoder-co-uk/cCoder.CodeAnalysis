// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Loggings;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Students;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Teachers;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.CodeAnalysis.Sample.Controllers.RuleViolations;

[ApiController]
[Route("api/[controller]")]
public sealed class InvalidStudentsController(
    IStudentOrchestrationService studentOrchestrationService,
    ITeacherOrchestrationService teacherOrchestrationService,
    ILoggingBroker loggingBroker) : ControllerBase
{
    [HttpGet("students")]
    public ActionResult<IQueryable<Student>> GetStudents()
    {
        try
        {
            return Ok(value: studentOrchestrationService.GetStudents());
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception);
            return StatusCode(statusCode: 500);
        }
    }

    [HttpGet("teachers")]
    public ActionResult<IQueryable<Teacher>> GetTeachers()
    {
        try
        {
            return Ok(value: teacherOrchestrationService.GetTeachers());
        }
        catch (Exception)
        {
            return StatusCode(statusCode: 500);
        }
    }
}