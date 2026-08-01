// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.CodeAnalysis.SampleWeb.Models.RuleViolations;

internal sealed class HttpRuleValidationException : Exception
{
}

internal sealed class HttpRuleAuthenticationException : Exception
{
}

internal sealed class HttpRuleAuthorizationException : Exception
{
}

internal sealed class HttpRuleConcurrencyException : Exception
{
}

internal sealed class HttpRulePreconditionException : Exception
{
}

internal sealed class HttpRuleUnsupportedMediaException : Exception
{
}