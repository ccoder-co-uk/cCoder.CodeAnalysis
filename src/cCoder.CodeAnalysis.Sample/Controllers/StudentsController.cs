// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Security;
using System.Security.Authentication;
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
            ActionResult<IQueryable<Student>> successResponse =
                Ok(value: studentOrchestrationService.GetStudents());

            loggingBroker.LogInformation(message: "Controller request succeeded.");

            return successResponse;
        }
        catch (ServiceValidationException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return BadRequest(error: "The request is invalid.");
        }
        catch (ArgumentException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return BadRequest(error: "The request is invalid.");
        }
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return Unauthorized(value: "Authentication failed.");
        }
        catch (UnauthorizedAccessException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return StatusCode(statusCode: 403, value: "Access is denied.");
        }
        catch (SecurityException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return StatusCode(statusCode: 403, value: "Access is denied.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception);
            return StatusCode(statusCode: 500, value: "The operation failed.");
        }
    }

    [HttpGet("{studentId:int}")]
    public ActionResult<Student> GetStudent(int studentId)
    {
        try
        {
            Student? student = studentOrchestrationService.GetStudent(
                studentId: studentId);

            if (student is null)
            {
                loggingBroker.LogWarning(message: "Student was not found.");
                return NotFound();
            }

            ActionResult<Student> successResponse = Ok(value: student);

            loggingBroker.LogInformation(message: "Controller request succeeded.");

            return successResponse;
        }
        catch (ServiceValidationException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return BadRequest(error: "The request is invalid.");
        }
        catch (ArgumentException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return BadRequest(error: "The request is invalid.");
        }
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return Unauthorized(value: "Authentication failed.");
        }
        catch (UnauthorizedAccessException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return StatusCode(statusCode: 403, value: "Access is denied.");
        }
        catch (SecurityException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return StatusCode(statusCode: 403, value: "Access is denied.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception);
            return StatusCode(statusCode: 500, value: "The operation failed.");
        }
    }

    [HttpPost]
    public async ValueTask<ActionResult<Student>> PostStudentAsync(
        Student newStudent)
    {
        try
        {
            Student addedStudent = await studentOrchestrationService
                .AddStudentAsync(newStudent: newStudent);

            ActionResult<Student> successResponse = CreatedAtAction(
                actionName: "GetStudent",
                routeValues: new { studentId = addedStudent.Id },
                value: addedStudent);

            loggingBroker.LogInformation(message: "Controller request succeeded.");

            return successResponse;
        }
        catch (ServiceValidationException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return BadRequest(error: "The request is invalid.");
        }
        catch (ArgumentException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return BadRequest(error: "The request is invalid.");
        }
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return Unauthorized(value: "Authentication failed.");
        }
        catch (UnauthorizedAccessException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return StatusCode(statusCode: 403, value: "Access is denied.");
        }
        catch (SecurityException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return StatusCode(statusCode: 403, value: "Access is denied.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception);
            return StatusCode(statusCode: 500, value: "The operation failed.");
        }
    }

    [HttpPut]
    public async ValueTask<ActionResult<Student>> PutStudentAsync(
        Student updatedStudent)
    {
        try
        {
            ActionResult<Student> successResponse = Ok(
                value: await studentOrchestrationService.UpdateStudentAsync(
                    updatedStudent: updatedStudent));

            loggingBroker.LogInformation(message: "Controller request succeeded.");

            return successResponse;
        }
        catch (ServiceValidationException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return BadRequest(error: "The request is invalid.");
        }
        catch (ArgumentException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return BadRequest(error: "The request is invalid.");
        }
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return Unauthorized(value: "Authentication failed.");
        }
        catch (UnauthorizedAccessException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return StatusCode(statusCode: 403, value: "Access is denied.");
        }
        catch (SecurityException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return StatusCode(statusCode: 403, value: "Access is denied.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception);
            return StatusCode(statusCode: 500, value: "The operation failed.");
        }
    }

    [HttpDelete("{studentId:int}")]
    public async ValueTask<IActionResult> DeleteStudentAsync(int studentId)
    {
        try
        {
            Student? student = studentOrchestrationService.GetStudent(
                studentId: studentId);

            if (student is null)
            {
                loggingBroker.LogWarning(message: "Student was not found.");
                return NotFound();
            }

            await studentOrchestrationService.DeleteStudentAsync(
                studentId: studentId);

            IActionResult successResponse = NoContent();

            loggingBroker.LogInformation(message: "Controller request succeeded.");

            return successResponse;
        }
        catch (ServiceValidationException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return BadRequest(error: "The request is invalid.");
        }
        catch (ArgumentException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return BadRequest(error: "The request is invalid.");
        }
        catch (AuthenticationException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return Unauthorized(value: "Authentication failed.");
        }
        catch (UnauthorizedAccessException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return StatusCode(statusCode: 403, value: "Access is denied.");
        }
        catch (SecurityException exception)
        {
            loggingBroker.LogWarning(exception: exception);
            return StatusCode(statusCode: 403, value: "Access is denied.");
        }
        catch (Exception exception)
        {
            loggingBroker.LogError(exception: exception);
            return StatusCode(statusCode: 500, value: "The operation failed.");
        }
    }
}