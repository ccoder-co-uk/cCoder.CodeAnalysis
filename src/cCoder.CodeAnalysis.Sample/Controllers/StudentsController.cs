// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Loggings;
using cCoder.CodeAnalysis.Sample.Exposures.Students;
using cCoder.CodeAnalysis.Sample.Models.Exceptions;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.CodeAnalysis.Sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StudentsController(
    IStudentManager studentManager,
    ILoggingBroker loggingBroker) : ControllerBase
{
    [HttpGet]
    public ActionResult<IQueryable<Student>> GetStudents()
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

    [HttpGet("{studentId:int}")]
    public ActionResult<Student> GetStudent(int studentId)
    {
        try
        {
            Student? student = studentManager.GetStudent(studentId: studentId);

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
            Student addedStudent = await studentManager.AddStudentAsync(newStudent: newStudent);

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
            return Ok(value: await studentManager.UpdateStudentAsync(updatedStudent: updatedStudent));
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
            Student? student = studentManager.GetStudent(studentId: studentId);

            if (student == null)
            {
                return NotFound();
            }

            await studentManager.DeleteStudentAsync(studentId: studentId);
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