// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.Sample.Exposures.SchoolImports;
using cCoder.CodeAnalysis.Sample.Models.Exceptions;
using cCoder.CodeAnalysis.Sample.Models.Schools;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.CodeAnalysis.Sample.Controllers;

[ApiController]
[Route("api/schools/import")]
public sealed class SchoolImportController(ISchoolImportManager importManager) : ControllerBase
{
    [HttpPost]
    public async ValueTask<IActionResult> PostSchoolAsync(School newSchool)
    {
        try
        {
            await importManager.ImportSchoolAsync(school: newSchool);
            return Accepted();
        }
        catch (ServiceValidationException)
        {
            return BadRequest();
        }
        catch (Exception)
        {
            return StatusCode(statusCode: 500);
        }
    }
}