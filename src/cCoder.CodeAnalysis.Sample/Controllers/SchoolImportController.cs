// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Brokers.Loggings;
using cCoder.CodeAnalysis.Sample.Exposures.SchoolImports;
using cCoder.CodeAnalysis.Sample.Models.Exceptions;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.CodeAnalysis.Sample.Controllers;

[ApiController]
[Route("api/schools/import")]
public sealed class SchoolImportController(
    ISchoolImportManager importManager,
    ILoggingBroker loggingBroker) : ControllerBase
{
    [HttpPost]
    public async ValueTask<IActionResult> PostSchoolAsync(School newSchool)
    {
        try
        {
            await importManager.ImportSchoolAsync(school: newSchool);
            return Accepted();
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