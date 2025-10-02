# SourceBackend Architecture Rules

## Overview
This is a .NET 8 backend using Controllers + CQRS Light + Pluggable JWT + RBAC.

**Projects:**
- `SourceBackend.Web` → HTTP host, Controllers, Auth, Swagger, DI
- `SourceBackend.Core` → Business logic, use cases (CQRS light)
- `SourceBackend.Infrastructure` → Adapters (DB, queues, email, etc.)

**Dependencies:** Web → Core, Infrastructure → Core. **Do not** add Web → Infrastructure.

---

## Core Principles

### Controllers (Thin)
Controllers parse/validate DTOs → call Core handler → return Result/DTO.

```csharp
[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<GetSomething.Result>> Get(
        [FromServices] GetSomething.Handler handler,
        [FromQuery] string? param,
        CancellationToken ct)
        => Ok(await handler.Handle(new GetSomething.Query(param), ct));
}
```

### CQRS Light (Core)
One file per action: `src/Core/Features/<Feature>/Queries|Commands/<Action>.cs`

```csharp
namespace SourceBackend.Core.Features.MyFeature.Queries;

public static class GetSomething
{
    public sealed record Query(string? Param);
    public sealed record Result(string Data, DateTimeOffset Timestamp);

    public sealed class Handler(TimeProvider clock, ICurrentUser currentUser)
    {
        public Task<Result> Handle(Query q, CancellationToken ct = default)
        {
            // Business logic here
            return Task.FromResult(new Result("data", clock.GetUtcNow()));
        }
    }
}
```

### Dependency Injection
- Register `TimeProvider.System` as singleton
- Register handlers explicitly as Scoped
- Inject concrete handler types via `[FromServices]` in controllers

```csharp
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddScoped<GetSomething.Handler>();
```

### Authentication & Authorization
Pluggable JWT with three modes:
- `LocalSymmetric` (HS256) - use `Auth:LocalSymmetric:Base64Key`
- `LocalAsymmetric` (RS256) - use public/private PEM
- `Oidc` - use `Auth:Oidc:Authority` + `Auth:Audience`

RBAC via `[Authorize(Roles="Admin,Manager")]`

### Time Handling
Use `TimeProvider` (testable) or `DateTimeOffset`. **Never** use `DateTime.Now/UtcNow`.

### Validation
Prefer DataAnnotations first. Use FluentValidation only when necessary.

### No Magic
- **No MediatR** (explicit handler injection)
- **No AutoMapper** (manual mapping)
- Constructor DI only
- Async/await with CancellationToken

---

## Feature Recipe (Step-by-step)

1. **Core** → Create `src/Core/Features/<Feature>/<Queries|Commands>/<Action>.cs`
   - `record Query` or `Command`
   - `record Result`
   - `sealed class Handler(...)` with `Task<Result> Handle(..., CancellationToken ct = default)`

2. **Web** → Update `src/Web/Controllers/<Feature>Controller.cs`
   - Inject handler via `[FromServices]`
   - Return `ActionResult<Result>`
   - Add `[Authorize]` if needed

3. **DI** → Register in `Program.cs`
   ```csharp
   builder.Services.AddScoped<MyAction.Handler>();
   ```

4. **Swagger** → Ensure `CustomSchemaIds` configured
   ```csharp
   builder.Services.AddSwaggerGen(c =>
   {
       c.CustomSchemaIds(t => (t.FullName ?? t.Name).Replace("+", "."));
   });
   ```

---

## Don'ts
- ❌ Don't use `DateTime.Now` or `DateTime.UtcNow`
- ❌ Don't depend on `HttpContext` in Core
- ❌ Don't expose domain entities in controllers
- ❌ Don't add MediatR or AutoMapper unless requested
- ❌ Don't commit secrets/PEMs/PFX
- ❌ Don't log tokens or PII
- ❌ Don't reference Infrastructure from Web (initially)

---

## Dos
- ✅ Use `TimeProvider` for testable time
- ✅ Use `ICurrentUser` for identity in Core
- ✅ Return Result/DTO records from controllers
- ✅ Use explicit handler registration
- ✅ Pass `CancellationToken` through
- ✅ Use records for immutability
- ✅ Apply SOLID, KISS, YAGNI

