using System.Security.Claims;
using Microsoft.Extensions.Options;
using SourceBackend.Core.Abstractions;

namespace SourceBackend.Web.Auth;

public sealed class CurrentUserFromHttpContext(IHttpContextAccessor acc, IOptions<AuthOptions> opt) : ICurrentUser
{
    public string? UserId => acc.HttpContext?.User.FindFirst("sub")?.Value
                             ?? acc.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public IReadOnlyCollection<string> Roles =>
        acc.HttpContext?.User.FindAll(opt.Value.RoleClaimType).Select(c => c.Value).ToArray()
        ?? Array.Empty<string>();
}