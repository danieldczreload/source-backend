# SourceBackend – Copilot Instructions

## Arquitectura esencial
- Solución .NET 8 dividida en `SourceBackend.Web`, `SourceBackend.Core` y `SourceBackend.Infrastructure`.
- `Web` solo referencia `Core`. `Infrastructure` solo referencia `Core`. Evitar `Web → Infrastructure`.
- Toda la lógica de negocio vive en `Core`. Web expone HTTP y wiring; Infrastructure implementa adapters.
- Código explícito: sin MediatR ni AutoMapper; usar mapping manual y constructor DI.

## Controladores y DTOs
- Ubicación: `src/Web/Controllers`. Decorar con `[ApiController]` y `[Route]`.
- Controladores delgados: validan DTOs/inputs, resuelven handlers desde `[FromServices]`, retornan `ActionResult<T>`.
- No exponer entidades de dominio. Usar `record` para DTOs y respuestas.
- Aplicar `[Authorize]` o `[Authorize(Roles="...")]` según RBAC.

## CQRS light en Core
- Una acción por archivo dentro de `src/Core/Features/<Feature>/<Queries|Commands>/<Action>.cs`.
- Estructura estándar:
  - `record Query|Command`
  - `record Result`
  - `sealed class Handler(...)` con dependencias inyectadas, `Task<Result> Handle(..., CancellationToken ct = default)`.
- Usar `TimeProvider` para tiempos; nada de `DateTime.Now/DateTime.UtcNow` directo.

## Dependencias y DI
- Registrar handlers explícitamente en `Program.cs` como `AddScoped<Feature.Handler>()`.
- Registrar `TimeProvider.System` como singleton.
- Mantener `ICurrentUser` en `Core.Abstractions`; implementación en Web leyendo claims desde `HttpContext` respetando `Auth:RoleClaimType`.

## Autenticación y autorización
- Soportar modos JWT pluggable: `LocalSymmetric`, `LocalAsymmetric`, `Oidc`.
- Respetar configuración en `AuthOptions` y evitar almacenar secretos en repo.
- Implementar RBAC mediante roles en claims; no usar ASP.NET Identity salvo petición explícita.

## Swagger y observabilidad
- Swagger habilitado solo en Development, con `CustomSchemaIds` para evitar colisiones de tipos anidados.
- Usar `Serilog` (Console + configuración) y `app.UseSerilogRequestLogging()`.
- No loggear tokens ni PII.

## Receta para nuevas features
1. Crear handler en Core (Query/Command/Result/Handler) con límites claros y dependencias mínimas.
2. Añadir endpoint en Web que consuma el handler y mapear inputs/outputs manualmente.
3. Registrar el handler en `Program.cs`.
4. Configurar `[Authorize]`/roles cuando aplique.
5. Agregar pruebas (idealmente en Core) usando `FakeTimeProvider` para tiempos deterministas.

## Estilo y buenas prácticas
- Usar `record` para models/DTOs inmutables, PascalCase para tipos, camelCase para parámetros y variables.
- Favor de async/await end-to-end, pasar `CancellationToken` siempre.
- Validaciones con DataAnnotations primero; FluentValidation solo si es necesario.
- Manejar errores con payload consistente `{ "error": "mensaje", "code": "opcional", "traceId": "opcional" }` o `ValidationProblemDetails` según corresponda.
