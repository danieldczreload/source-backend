# Database Configuration - Initial Setup

> **Note**: This document covers the initial database configuration. For creating new entities and migrations, see `CREATING-ENTITIES.md`.

## ✅ What Has Been Configured

### 1. NuGet Packages Installed
- ✅ `Microsoft.EntityFrameworkCore.SqlServer` (8.0.10)
- ✅ `Microsoft.EntityFrameworkCore.Design` (8.0.10)

### 2. Files Created

#### Infrastructure Layer
- ✅ `src/Infrastructure/Data/ApplicationDbContext.cs` - Main DbContext
- ✅ `src/Infrastructure/Data/Configurations/` - Folder for entity configurations
- ✅ `src/Infrastructure/Data/README.md` - Complete documentation

#### Web Layer
- ✅ Connection string configured in `appsettings.json` (placeholder)
- ✅ Real connection string saved in **user-secrets** (secure, not committed)
- ✅ Infrastructure project reference added
- ✅ DbContext registered in DI in `Program.cs`

### 3. Security
- ✅ User-secrets initialized with ID: `234234ae-9902-4e7a-9030-058eed1426e6`
- ✅ Real credentials saved in user-secrets (not in Git)
- ✅ Safe placeholder in appsettings.json

## 🎯 Approach Adopted: KISS/YAGNI

**Generic Repository pattern is NOT used** to avoid unnecessary boilerplate.

### KISS - Keep It Simple, Stupid
> "Prefer simple and direct solutions over complex ones"

EF Core **is already an abstraction** over the database. Adding generic repositories on top of it is redundant.

### YAGNI - You Aren't Gonna Need It
> "Don't add functionality just in case you need it in the future"

Use `ApplicationDbContext` directly. Only create specific repositories **when you really need them**.

## 📝 How to Use the Database

### Queries (Read)
```csharp
public sealed class Handler(ApplicationDbContext db)
{
    public async Task<Result> Handle(Query q, CancellationToken ct)
    {
        var data = await db.MyEntities
            .AsNoTracking()  // ← Important for queries
            .Where(x => x.Active)
            .Select(x => new MyDto(x.Id, x.Name))
            .ToListAsync(ct);
            
        return new Result(data);
    }
}
```

### Commands (Write)
```csharp
public sealed class Handler(ApplicationDbContext db, TimeProvider clock)
{
    public async Task<Result> Handle(Command cmd, CancellationToken ct)
    {
        var entity = new MyEntity 
        { 
            Name = cmd.Name,
            CreatedAt = clock.GetUtcNow()
        };
        
        db.MyEntities.Add(entity);
        await db.SaveChangesAsync(ct);
        
        return new Result(entity.Id);
    }
}
```

## 🔧 Next Steps

### Creating New Entities

See **`CREATING-ENTITIES.md`** for a complete step-by-step guide on:
- Creating entity classes
- Configuring entities with Fluent API
- Adding DbSets to the context
- Creating and applying migrations
- Using entities in handlers

## 🔐 Managing Connection Strings

### View current connection string
```bash
dotnet user-secrets list --project src/Web/SourceBackend.Web.csproj
```

### Update connection string
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-new-connection-string" --project src/Web/SourceBackend.Web.csproj
```

### In Production
Configure the environment variable:
```
ConnectionStrings__DefaultConnection="your-production-connection-string"
```

## 📚 Related Documentation

- **`CREATING-ENTITIES.md`** - Step-by-step guide for creating new entities and migrations
- `src/Infrastructure/Data/README.md` - Complete data layer documentation
- `.cursorrules` - Project architecture rules (Section §12 Infrastructure)

## ✨ Benefits Summary

✅ **Simple**: Use DbContext directly, no unnecessary layers  
✅ **Clean**: No generic repository boilerplate  
✅ **Flexible**: Create specific abstractions only when needed  
✅ **Testable**: Mock `ApplicationDbContext` or use InMemory provider  
✅ **Secure**: Secrets in user-secrets, not in code
