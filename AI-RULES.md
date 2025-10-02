# AI Coding Assistant Rules

This project includes configuration files for multiple AI coding assistants to ensure consistent code generation following our architecture.

## Files for Different Tools

### 🎯 Cursor
**File:** `.cursorrules` (root directory)

Cursor automatically reads this file. No additional configuration needed.

### 🤖 Claude Code
**Files:** 
- `CLAUDE.md` (root directory) - Complete rules
- `.claude/rules/architecture.md` - Condensed version
- `.claude/README.md` - Quick reference

Claude Code and similar tools will discover these files automatically.

### 🔧 Other AI Tools
Use either `.cursorrules` or `CLAUDE.md` - they contain the same architectural guidelines.

---

## What These Rules Define

All rule files enforce the same architecture:

### Architecture
- **CQRS Light** - Queries and Commands in Core, one file per action
- **Thin Controllers** - Parse DTOs → Call handlers → Return results
- **Clean Architecture** - Web → Core ← Infrastructure (no Web → Infrastructure)
- **Explicit Code** - No MediatR, no AutoMapper, manual DI

### Authentication & Authorization
- **Pluggable JWT** - LocalSymmetric (HS256), LocalAsymmetric (RS256), or OIDC
- **RBAC** - Role-based with `[Authorize(Roles="...")]`
- **ICurrentUser** - Abstract identity from HttpContext

### Best Practices
- Use `TimeProvider` (testable time), not `DateTime.Now`
- Use `record` for DTOs and immutability
- Pass `CancellationToken` through async calls
- DataAnnotations for validation
- Serilog for logging
- SOLID, KISS, YAGNI

### Project Structure
```
src/
  Web/              → Controllers, Auth, DI, Swagger
    Controllers/
    Auth/
    Program.cs
  Core/             → Business logic, handlers
    Features/
      <Feature>/
        Queries/    → GetX.cs (Query, Result, Handler)
        Commands/   → CreateX.cs (Command, Result, Handler)
    Abstractions/   → ICurrentUser, ports
  Infrastructure/   → DB, queues, email adapters
```

---

## Feature Recipe

When creating a new feature, AI assistants will follow this pattern:

1. **Core** - Create handler
   ```
   src/Core/Features/<Feature>/Queries/<Action>.cs
   ```
   
2. **Web** - Create/update controller
   ```
   src/Web/Controllers/<Feature>Controller.cs
   ```

3. **DI** - Register in Program.cs
   ```csharp
   builder.Services.AddScoped<MyAction.Handler>();
   ```

4. **Auth** - Add `[Authorize]` if needed

---

## For Team Members

### Using Cursor
Just open the project - `.cursorrules` is automatically loaded.

### Using Claude Code
The `.claude/` directory is automatically discovered. You can also reference `CLAUDE.md` explicitly.

### Using GitHub Copilot or Other Tools
These tools don't natively support rule files, but you can:
1. Reference `CLAUDE.md` or `.cursorrules` in your prompts
2. Copy relevant sections when asking for code generation
3. Use them as documentation for code reviews

---

## Updating Rules

If architecture changes:
1. Update `.cursorrules` (Cursor)
2. Update `CLAUDE.md` (Claude Code root)
3. Update `.claude/rules/architecture.md` (Claude Code condensed)

Keep all three in sync to ensure consistency across tools.

---

## Questions?

These rules are **prescriptive** - they tell AI tools how to generate code for this project. If you're unsure about a pattern, check the examples in sections 18-19 of any rule file.

