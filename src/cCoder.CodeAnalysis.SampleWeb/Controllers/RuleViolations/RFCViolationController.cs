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
        [FromBody] HttpRuleModel model) =>
        Ok(model);

    [HttpDelete]
    public IActionResult Delete(int key) =>
        Ok();

    [HttpGet]
    public IActionResult Get() =>
        NoContent();

    [HttpPut]
    public IActionResult Put(
        int key,
        HttpRuleModel model) =>
        NoContent();

    [HttpPost("validation")]
    public void Validate()
    {
        throw new HttpRuleValidationException();
    }

    [HttpPost("authentication")]
    public void Authenticate()
    {
        throw new HttpRuleAuthenticationException();
    }

    [HttpPost("authorization")]
    public void Authorize()
    {
        throw new HttpRuleAuthorizationException();
    }

    [HttpGet("{key:int}")]
    public IActionResult Get(int key) =>
        Ok(FindModel(key: key));

    [HttpPut("concurrency")]
    public void UpdateConcurrency()
    {
        throw new HttpRuleConcurrencyException();
    }

    [HttpGet("unsupported")]
    public void GetUnsupportedFunction()
    {
        throw new NotImplementedException();
    }

    [HttpPost("disclosure")]
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