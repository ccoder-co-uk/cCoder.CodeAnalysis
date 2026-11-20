// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Security;
using System.Security.Authentication;
using cCoder.CodeAnalysis.Exposures;
using cCoder.CodeAnalysis.Sample.Brokers.Loggings;
using cCoder.CodeAnalysis.Sample.Models.Exceptions;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using cCoder.CodeAnalysis.Sample.Services.Aggregations.SchoolImports;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.CodeAnalysis.Sample.Controllers;

[ApiController]
[Route("api/schools/import")]
public sealed class SchoolImportController(
    ISchoolImportAggregationService schoolImportAggregationService,
    ILoggingBroker loggingBroker) : ControllerBase, ICompositionExposure
{
    [HttpPost]
    public async ValueTask<IActionResult> PostSchoolAsync(School newSchool)
    {
        try
        {
            await schoolImportAggregationService.ImportSchoolAsync(
                school: newSchool);

            IActionResult successResponse = Accepted();

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