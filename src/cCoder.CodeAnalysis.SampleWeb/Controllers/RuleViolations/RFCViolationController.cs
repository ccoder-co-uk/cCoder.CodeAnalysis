// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.CodeAnalysis.SampleWeb.Models.RuleViolations;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.CodeAnalysis.SampleWeb.Controllers.RuleViolations;

[ApiController]
[Route("api/rfc-violations")]
public sealed class RFCViolationController : ODataController
{
    [HttpPost]
    public IActionResult Post(
        [FromBody] HttpRuleModel model)
    {
        try
        {
            return Ok(model);
        }
        catch (Exception)
        {
            return StatusCode(statusCode: 500);
        }
    }

    [HttpDelete]
    public IActionResult Delete(int key)
    {
        try
        {
            return Ok();
        }
        catch (Exception)
        {
            return StatusCode(statusCode: 500);
        }
    }

    [HttpGet]
    public IActionResult Get()
    {
        try
        {
            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(statusCode: 500);
        }
    }

    [HttpPut]
    public IActionResult Put(
        int key,
        HttpRuleModel model)
    {
        try
        {
            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(statusCode: 500);
        }
    }

    [HttpGet("validation")]
    public IActionResult Validate()
    {
        try
        {
            throw new HttpRuleValidationException();
        }
        catch (HttpRuleValidationException)
        {
            return StatusCode(statusCode: 500);
        }
    }

    [HttpGet("authentication")]
    public IActionResult Authenticate()
    {
        try
        {
            throw new HttpRuleAuthenticationException();
        }
        catch (HttpRuleAuthenticationException)
        {
            return StatusCode(statusCode: 500);
        }
    }

    [HttpGet("authorization")]
    public IActionResult Authorize()
    {
        try
        {
            throw new HttpRuleAuthorizationException();
        }
        catch (HttpRuleAuthorizationException)
        {
            return StatusCode(statusCode: 500);
        }
    }

    [HttpGet("{key:int}")]
    public IActionResult Get(int key)
    {
        try
        {
            return Ok(FindModel(key: key));
        }
        catch (Exception)
        {
            return StatusCode(statusCode: 500);
        }
    }

    [HttpPut("concurrency")]
    public IActionResult UpdateConcurrency()
    {
        try
        {
            throw new HttpRuleConcurrencyException();
        }
        catch (HttpRuleConcurrencyException)
        {
            return StatusCode(statusCode: 500);
        }
    }

    [HttpGet("unsupported")]
    public IActionResult GetUnsupportedFunction()
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (NotImplementedException)
        {
            return StatusCode(statusCode: 500);
        }
    }

    [HttpGet("disclosure")]
    public IActionResult DiscloseValidationFailure()
    {
        try
        {
            throw new HttpRuleValidationException();
        }
        catch (HttpRuleValidationException exception)
        {
            return BadRequest(error: exception.Message);
        }
    }

    [HttpGet("unexpected")]
    public IActionResult GetUnexpectedFailure()
    {
        try
        {
            throw new Exception();
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }

    private static HttpRuleModel? FindModel(int key) =>
        key > 0
            ? new HttpRuleModel { Id = key }
            : null;

}