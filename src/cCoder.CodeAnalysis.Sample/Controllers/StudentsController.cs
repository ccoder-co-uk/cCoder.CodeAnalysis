// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Loggings;
using cCoder.CodeAnalysis.Sample.Models.Exceptions;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Orchestrations.Students;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.CodeAnalysis.Sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StudentsController(
    IStudentOrchestrationService studentOrchestrationService,
    ILoggingBroker loggingBroker) : ControllerBase
{
    [HttpGet]
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

    [HttpGet("{studentId:int}")]
    public ActionResult<Student> GetStudent(int studentId)
    {
        try
        {
            Student? student = studentOrchestrationService.GetStudent(studentId: studentId);

            return (student == null)
                ? ((ActionResult<Student>)NotFound())
                : ((ActionResult<Student>)Ok(value: student));
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception);
            return StatusCode(statusCode: 500);
        }
    }

    [HttpPost]
    public async ValueTask<ActionResult<Student>> PostStudentAsync(Student newStudent)
    {
        try
        {
            Student addedStudent = await studentOrchestrationService.AddStudentAsync(newStudent: newStudent);

            return CreatedAtAction(
                actionName: "GetStudent",
                routeValues: new { studentId = addedStudent.Id },
                value: addedStudent
            );
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception);
            return StatusCode(statusCode: 500);
        }
    }

    [HttpPut]
    public async ValueTask<ActionResult<Student>> PutStudentAsync(Student updatedStudent)
    {
        try
        {
            return Ok(value: await studentOrchestrationService.UpdateStudentAsync(updatedStudent: updatedStudent));
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception);
            return StatusCode(statusCode: 500);
        }
    }

    [HttpDelete("{studentId:int}")]
    public async ValueTask<IActionResult> DeleteStudentAsync(int studentId)
    {
        try
        {
            Student? student = studentOrchestrationService.GetStudent(studentId: studentId);

            if (student == null)
            {
                return NotFound();
            }

            await studentOrchestrationService.DeleteStudentAsync(studentId: studentId);
            return NoContent();
        }
        catch (ServiceValidationException exception)
        {
            loggingBroker.LogError(exception: exception);
            return BadRequest();
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception);
            return StatusCode(statusCode: 500);
        }
    }
}