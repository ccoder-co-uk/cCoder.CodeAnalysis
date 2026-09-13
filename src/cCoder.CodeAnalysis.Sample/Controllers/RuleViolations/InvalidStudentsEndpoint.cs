// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Loggings;
using cCoder.CodeAnalysis.Sample.Exposures.Students;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.CodeAnalysis.Sample.Controllers.RuleViolations;

[ApiController]
[Route("api/students-invalid-name")]
public sealed class InvalidStudentsEndpoint(
    IStudentManager studentManager,
    ILoggingBroker loggingBroker) : ControllerBase
{
    [HttpGet]
    public ActionResult<IQueryable<Student>> Get()
    {
        try
        {
            return Ok(value: studentManager.GetStudents());
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception);
            return StatusCode(statusCode: 500);
        }
    }
}