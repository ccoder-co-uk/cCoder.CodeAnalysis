// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;

namespace StatusCodeProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AppController : ControllerBase
{
    [HttpGet]
    public IActionResult GetIsAdmin()
    {
        try
        {
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(statusCode: StatusCodes.Status403Forbidden);
        }
        catch (Exception)
        {
            return StatusCode(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    public IActionResult Post()
    {
        try
        {
            return StatusCode(
                statusCode: StatusCodes.Status201Created,
                value: new { Id = 1 });
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(statusCode: StatusCodes.Status403Forbidden);
        }
        catch (Exception)
        {
            return StatusCode(statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
