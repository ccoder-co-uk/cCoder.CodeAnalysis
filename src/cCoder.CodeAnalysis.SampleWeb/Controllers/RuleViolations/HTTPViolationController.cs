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
    public IActionResult GetHeaders() =>
        Ok(new HttpRuleModel());

    [HttpGet("invalid-status")]
    public IActionResult GetInvalidStatus() =>
        StatusCode(statusCode: 799);

    [HttpPost("no-content")]
    public IActionResult PostNoContent(HttpRuleModel model) =>
        StatusCode(statusCode: 204, value: model);

    [HttpPost("unsupported-media")]
    public void PostUnsupportedMedia()
    {
        throw new HttpRuleUnsupportedMediaException();
    }

    [HttpPut("precondition")]
    public void PutPrecondition()
    {
        throw new HttpRulePreconditionException();
    }
}