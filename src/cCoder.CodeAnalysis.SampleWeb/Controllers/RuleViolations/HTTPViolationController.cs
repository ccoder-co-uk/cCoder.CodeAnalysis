// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.SampleWeb.Models.RuleViolations;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.CodeAnalysis.SampleWeb.Controllers.RuleViolations;

[ApiController]
[Route("api/http-violations")]
public sealed class HTTPViolationController : ControllerBase
{
    [HttpHead]
    public IActionResult GetHeaders()
    {
        try
        {
            return Ok(new HttpRuleModel());
        }
        catch (Exception)
        {
            return StatusCode(statusCode: 500);
        }
    }

    [HttpGet("invalid-status")]
    public IActionResult GetInvalidStatus()
    {
        try
        {
            return StatusCode(statusCode: 799);
        }
        catch (Exception)
        {
            return StatusCode(statusCode: 500);
        }
    }

    [HttpPost("no-content")]
    public IActionResult PostNoContent(HttpRuleModel model)
    {
        try
        {
            return StatusCode(statusCode: 204, value: model);
        }
        catch (Exception)
        {
            return StatusCode(statusCode: 500);
        }
    }

    [HttpPost("unsupported-media")]
    public IActionResult PostUnsupportedMedia()
    {
        try
        {
            throw new HttpRuleUnsupportedMediaException();
        }
        catch (HttpRuleUnsupportedMediaException)
        {
            return StatusCode(statusCode: 500);
        }
    }

    [HttpPut("precondition")]
    public IActionResult PutPrecondition()
    {
        try
        {
            throw new HttpRulePreconditionException();
        }
        catch (HttpRulePreconditionException)
        {
            return StatusCode(statusCode: 500);
        }
    }
}