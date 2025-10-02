# Claude Code Configuration

This directory contains architecture rules and conventions for Claude Code users.

## Files

- `rules/architecture.md` - Core architecture principles and patterns for this project

## How to Use

When using Claude Code (or any Claude-based coding assistant), these files will be automatically discovered and used as context to ensure generated code follows the project's conventions.

### For Developers

If you're using:
- **Cursor** → Use `.cursorrules` in the root
- **Claude Code** → This `.claude/` folder is for you
- **Other AI tools** → Use `CLAUDE.md` in the root

All files contain the same architectural rules adapted for each tool.

## Quick Reference

**Stack:** .NET 8, ASP.NET Core, Controllers, CQRS Light, JWT Auth, RBAC

**Key Patterns:**
- Thin Controllers → Core Handlers → DTOs
- One file per use case (Query or Command)
- Explicit DI, no magic (no MediatR/AutoMapper)
- TimeProvider for testable time
- ICurrentUser for identity

**Projects:**
- `SourceBackend.Web` → HTTP, Controllers, Auth
- `SourceBackend.Core` → Business logic
- `SourceBackend.Infrastructure` → Adapters (DB, etc.)

