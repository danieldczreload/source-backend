using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SourceBackend.Web.Auth;

namespace SourceBackend.Web.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest req, [FromServices] ITokenIssuer issuer)
    {
        var token = issuer.Issue(req.Sub ?? Guid.NewGuid().ToString(), req.Roles ?? new[] { "User" }, TimeSpan.FromHours(1));
        return string.IsNullOrEmpty(token)
            ? BadRequest(new { error = "Token issuing disabled in current Auth mode." })
            : Ok(new { access_token = token });
    }
}
public sealed record LoginRequest(string? Sub, string[]? Roles);