using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SourceBackend.Web.Controllers;

[ApiController]
[Route("me")]
[Authorize]
public sealed class MeController : ControllerBase
{
    private readonly IOptions<SourceBackend.Web.Auth.AuthOptions> _auth;
    public MeController(IOptions<SourceBackend.Web.Auth.AuthOptions> auth) => _auth = auth;

    [HttpGet]
    public IActionResult Get()
    {
        var u = HttpContext.User;
        var sub = u.FindFirst("sub")?.Value ?? u.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roles = u.FindAll(_auth.Value.RoleClaimType).Select(c => c.Value);
        return Ok(new { sub, roles });
    }
}