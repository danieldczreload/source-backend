using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SourceBackend.Web.Controllers;

[ApiController]
[Route("admin")]
[Authorize(Roles = "Admin")]
public sealed class AdminController : ControllerBase
{
    [HttpGet("hello")]
    public IActionResult Hello() => Ok(new { message = "Hello, Admin!" });
}