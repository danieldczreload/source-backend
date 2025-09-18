using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SourceBackend.Core.Features.Hello.Queries;
using SourceBackend.Core.Features.Hello.Commands;

namespace SourceBackend.Web.Controllers;

[ApiController]
[Route("hello")]
[AllowAnonymous]
public sealed class HelloController : ControllerBase
{
    // GET /hello?name=Daniel  -> Query
    [HttpGet]
    public async Task<ActionResult<GetHello.Result>> Get(
        [FromServices] GetHello.Handler handler,
        [FromQuery] string? name,
        CancellationToken ct)
    {
        var res = await handler.Handle(new GetHello.Query(name), ct);
        return Ok(res);
    }

    public sealed record PostHelloDto(string? Name);

    // POST /hello { "name": "Daniel" } -> Command
    [HttpPost]
    public async Task<ActionResult<SendHello.Result>> Post(
        [FromServices] SendHello.Handler handler,
        [FromBody] PostHelloDto dto,
        CancellationToken ct)
    {
        var res = await handler.Handle(new SendHello.Command(dto?.Name), ct);
        return Created("/hello", res);
    }
}