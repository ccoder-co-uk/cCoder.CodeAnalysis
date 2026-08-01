// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Identity;

namespace cCoder.CodeAnalysis.SampleWeb.Controllers.RuleViolations;

internal sealed class OWASPViolationController
{
    public string HashPasswordDirectly(string password)
    {
        PasswordHasher<object> hasher = new();

        return hasher.HashPassword(
            user: new object(),
            password: password);
    }

    public string GenerateSecurityToken() =>
        Guid
            .NewGuid()
            .ToString(format: "N");
}