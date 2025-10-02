# Creating New Entities - Developer Guide

This guide walks you through creating new entities, configurations, and applying migrations.

## 📋 Prerequisites

- Database connection configured (see `DATABASE-SETUP.md`)
- `dotnet-ef` tool installed globally
- Understanding of the project's architecture (CQRS + EF Core)

---

## 🏗️ Step-by-Step Guide

### Step 1: Create the Entity Class

All entities should inherit from `BaseEntity` to get audit fields automatically.

**Location**: `src/Core/Entities/YourEntity.cs`

```csharp
namespace SourceBackend.Core.Entities;

/// <summary>
/// Your entity description.
/// </summary>
public sealed class YourEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Add your specific properties here
}
```

**What you get from `BaseEntity`:**
- `Id` (Guid) - Primary key
- `CreatedAt` (DateTimeOffset) - When created
- `CreatedBy` (Guid?) - Who created it
- `UpdatedAt` (DateTimeOffset?) - When last updated
- `UpdatedBy` (Guid?) - Who updated it
- `DeletedAt` (DateTimeOffset?) - When soft deleted
- `DeletedBy` (Guid?) - Who deleted it

---

### Step 2: Create Entity Configuration

**Location**: `src/Infrastructure/Data/Configurations/YourEntityConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SourceBackend.Core.Entities;

namespace SourceBackend.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for YourEntity.
/// </summary>
public sealed class YourEntityConfiguration : BaseEntityConfiguration<YourEntity>
{
    public override void Configure(EntityTypeBuilder<YourEntity> builder)
    {
        // Apply base configuration (Id, audit fields, PK)
        base.Configure(builder);
        
        // Table name
        builder.ToTable("your_entities");
        
        // Name - required with max length
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        // Description - optional
        builder.Property(e => e.Description)
            .HasMaxLength(1000);
        
        // Price - decimal with precision
        builder.Property(e => e.Price)
            .HasPrecision(18, 2);
        
        // IsActive - required with default value
        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        // Indexes
        builder.HasIndex(e => e.Name);
        
        // Unique constraints
        // builder.HasIndex(e => e.Email).IsUnique();
        
        // Relationships (if needed)
        // builder.HasOne(e => e.Category)
        //     .WithMany(c => c.Products)
        //     .HasForeignKey(e => e.CategoryId);
    }
}
```

**Configuration Tips:**

| Property Type | Configuration Example |
|--------------|----------------------|
| Required string | `.IsRequired().HasMaxLength(200)` |
| Optional string | `.HasMaxLength(500)` |
| Decimal/Money | `.HasPrecision(18, 2)` |
| Boolean with default | `.IsRequired().HasDefaultValue(true)` |
| Unique index | `.HasIndex(e => e.Email).IsUnique()` |
| Regular index | `.HasIndex(e => e.Name)` |
| Enum as string | `.HasConversion<string>()` |

---

### Step 3: Add DbSet to ApplicationDbContext

**Location**: `src/Infrastructure/Data/ApplicationDbContext.cs`

```csharp
// Add your DbSet
public DbSet<YourEntity> YourEntities => Set<YourEntity>();
```

**Example:**
```csharp
public sealed class ApplicationDbContext : DbContext
{
    // ... constructor ...
    
    // DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();  // ← Add new DbSet here
    public DbSet<Category> Categories => Set<Category>();
    
    // ... OnModelCreating ...
}
```

---

### Step 4: Create Migration

```bash
dotnet ef migrations add CreateYourEntity \
  --project src/Infrastructure/SourceBackend.Infrastructure.csproj \
  --startup-project src/Web/SourceBackend.Web.csproj
```

**Migration naming conventions:**
- `CreateYourEntity` - For creating new tables
- `AddFieldToYourEntity` - For adding fields
- `UpdateYourEntityIndexes` - For index changes
- `AddYourEntityRelationship` - For FK relationships

---

### Step 5: Review Generated Migration

Check the generated file in `src/Infrastructure/Migrations/`:

```csharp
public partial class CreateYourEntity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Review the SQL that will be executed
        migrationBuilder.CreateTable(...);
    }
    
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Rollback logic
        migrationBuilder.DropTable(...);
    }
}
```

**Verify:**
- ✅ Table name is correct
- ✅ All fields are present
- ✅ Data types are appropriate
- ✅ Indexes are created
- ✅ Relationships are correct

---

### Step 6: Apply Migration to Database

```bash
dotnet ef database update \
  --project src/Infrastructure/SourceBackend.Infrastructure.csproj \
  --startup-project src/Web/SourceBackend.Web.csproj
```

This will:
1. Connect to the database (using connection string from user-secrets)
2. Check `__EFMigrationsHistory` table
3. Apply pending migrations
4. Update migration history

---

## 🔄 Common Migration Commands

### View Migration SQL (without executing)
```bash
dotnet ef migrations script \
  --project src/Infrastructure/SourceBackend.Infrastructure.csproj \
  --startup-project src/Web/SourceBackend.Web.csproj
```

### Update to Specific Migration
```bash
dotnet ef database update YourMigrationName \
  --project src/Infrastructure/SourceBackend.Infrastructure.csproj \
  --startup-project src/Web/SourceBackend.Web.csproj
```

### Rollback Last Migration
```bash
# Remove from code (if not applied to DB yet)
dotnet ef migrations remove \
  --project src/Infrastructure/SourceBackend.Infrastructure.csproj \
  --startup-project src/Web/SourceBackend.Web.csproj

# Rollback from database
dotnet ef database update PreviousMigrationName \
  --project src/Infrastructure/SourceBackend.Infrastructure.csproj \
  --startup-project src/Web/SourceBackend.Web.csproj
```

### List All Migrations
```bash
dotnet ef migrations list \
  --project src/Infrastructure/SourceBackend.Infrastructure.csproj \
  --startup-project src/Web/SourceBackend.Web.csproj
```

---

## 💡 Using Entities in Handlers

### Query (Read)
```csharp
// src/Core/Features/YourEntity/Queries/GetYourEntities.cs
namespace SourceBackend.Core.Features.YourEntity.Queries;

public static class GetYourEntities
{
    public sealed record Query(bool ActiveOnly);
    public sealed record Result(IReadOnlyList<YourEntityDto> Items);
    public sealed record YourEntityDto(Guid Id, string Name, decimal Price);

    public sealed class Handler(ApplicationDbContext db)
    {
        public async Task<Result> Handle(Query q, CancellationToken ct)
        {
            var query = db.YourEntities.AsNoTracking();
            
            if (q.ActiveOnly)
                query = query.Where(e => e.IsActive);
            
            var items = await query
                .Select(e => new YourEntityDto(e.Id, e.Name, e.Price))
                .ToListAsync(ct);
                
            return new Result(items);
        }
    }
}
```

### Command (Create)
```csharp
// src/Core/Features/YourEntity/Commands/CreateYourEntity.cs
namespace SourceBackend.Core.Features.YourEntity.Commands;

public static class CreateYourEntity
{
    public sealed record Command(string Name, string Description, decimal Price);
    public sealed record Result(Guid Id, DateTimeOffset CreatedAt);

    public sealed class Handler(
        ApplicationDbContext db, 
        TimeProvider clock,
        ICurrentUser currentUser)
    {
        public async Task<Result> Handle(Command cmd, CancellationToken ct)
        {
            var entity = new YourEntity
            {
                Id = Guid.NewGuid(),
                Name = cmd.Name,
                Description = cmd.Description,
                Price = cmd.Price,
                IsActive = true,
                CreatedAt = clock.GetUtcNow(),
                CreatedBy = Guid.Parse(currentUser.UserId ?? Guid.Empty.ToString())
            };
            
            db.YourEntities.Add(entity);
            await db.SaveChangesAsync(ct);
            
            return new Result(entity.Id, entity.CreatedAt);
        }
    }
}
```

### Command (Update)
```csharp
// src/Core/Features/YourEntity/Commands/UpdateYourEntity.cs
namespace SourceBackend.Core.Features.YourEntity.Commands;

public static class UpdateYourEntity
{
    public sealed record Command(Guid Id, string Name, string Description, decimal Price);
    public sealed record Result(bool Success);

    public sealed class Handler(
        ApplicationDbContext db,
        TimeProvider clock,
        ICurrentUser currentUser)
    {
        public async Task<Result> Handle(Command cmd, CancellationToken ct)
        {
            var entity = await db.YourEntities
                .FirstOrDefaultAsync(e => e.Id == cmd.Id, ct);
                
            if (entity is null)
                throw new KeyNotFoundException($"Entity {cmd.Id} not found");
            
            entity.Name = cmd.Name;
            entity.Description = cmd.Description;
            entity.Price = cmd.Price;
            entity.UpdatedAt = clock.GetUtcNow();
            entity.UpdatedBy = Guid.Parse(currentUser.UserId ?? Guid.Empty.ToString());
            
            await db.SaveChangesAsync(ct);
            
            return new Result(true);
        }
    }
}
```

### Command (Soft Delete)
```csharp
// src/Core/Features/YourEntity/Commands/DeleteYourEntity.cs
namespace SourceBackend.Core.Features.YourEntity.Commands;

public static class DeleteYourEntity
{
    public sealed record Command(Guid Id);
    public sealed record Result(bool Success);

    public sealed class Handler(
        ApplicationDbContext db,
        TimeProvider clock,
        ICurrentUser currentUser)
    {
        public async Task<Result> Handle(Command cmd, CancellationToken ct)
        {
            var entity = await db.YourEntities
                .FirstOrDefaultAsync(e => e.Id == cmd.Id, ct);
                
            if (entity is null)
                throw new KeyNotFoundException($"Entity {cmd.Id} not found");
            
            // Soft delete
            entity.DeletedAt = clock.GetUtcNow();
            entity.DeletedBy = Guid.Parse(currentUser.UserId ?? Guid.Empty.ToString());
            
            await db.SaveChangesAsync(ct);
            
            return new Result(true);
        }
    }
}
```

---

## 🎯 Best Practices

### 1. **Always Use `AsNoTracking()` for Queries**
```csharp
// ✅ GOOD: Read-only query
var items = await db.Products.AsNoTracking().ToListAsync(ct);

// ❌ BAD: Tracking when not needed
var items = await db.Products.ToListAsync(ct);
```

### 2. **Project to DTOs in Queries**
```csharp
// ✅ GOOD: Project to DTO, only fetch needed columns
var items = await db.Products
    .AsNoTracking()
    .Select(p => new ProductDto(p.Id, p.Name, p.Price))
    .ToListAsync(ct);

// ❌ BAD: Fetch full entity when only need few fields
var items = await db.Products.AsNoTracking().ToListAsync(ct);
```

### 3. **Use Audit Fields**
```csharp
// ✅ GOOD: Set audit fields
entity.CreatedAt = clock.GetUtcNow();
entity.CreatedBy = Guid.Parse(currentUser.UserId);

// ❌ BAD: Ignore audit fields
entity.CreatedAt = DateTimeOffset.UtcNow; // Don't use this, use TimeProvider
```

### 4. **Prefer Soft Delete**
```csharp
// ✅ GOOD: Soft delete (preserves history)
entity.DeletedAt = clock.GetUtcNow();
entity.DeletedBy = currentUserId;

// ❌ BAD: Hard delete (loses data)
db.YourEntities.Remove(entity);
```

### 5. **Always Pass CancellationToken**
```csharp
// ✅ GOOD
await db.SaveChangesAsync(ct);

// ❌ BAD
await db.SaveChangesAsync();
```

---

## 🚨 Troubleshooting

### Error: "No migrations found"
**Solution**: Make sure you're in the project root and using correct paths.

### Error: "Could not find part of the path 'Migrations'"
**Solution**: The folder will be created automatically on first migration.

### Error: "Cannot insert duplicate key"
**Solution**: Check unique constraints and indexes. Use `FirstOrDefaultAsync` before inserting.

### Error: "A connection was successfully established... but then failed"
**Solution**: 
1. Check connection string: `dotnet user-secrets list --project src/Web`
2. Verify SQL Server is accessible
3. Check firewall settings

### Migration applied but table not created
**Solution**: Check the migration history:
```sql
SELECT * FROM __EFMigrationsHistory
```

---

## 📚 Additional Resources

- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Fluent API Configuration](https://learn.microsoft.com/en-us/ef/core/modeling/)
- Project Architecture Rules: `.cursorrules` file
- Database Setup: `Documentation/DATABASE-SETUP.md`
- Data Layer Docs: `src/Infrastructure/Data/README.md`

---

## 📝 Quick Reference

### Entity Template
```csharp
public sealed class MyEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}
```

### Configuration Template
```csharp
public sealed class MyEntityConfiguration : BaseEntityConfiguration<MyEntity>
{
    public override void Configure(EntityTypeBuilder<MyEntity> builder)
    {
        base.Configure(builder);
        builder.ToTable("my_entities");
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
    }
}
```

### Commands Cheat Sheet
```bash
# Create migration
dotnet ef migrations add MigrationName --project src/Infrastructure/SourceBackend.Infrastructure.csproj --startup-project src/Web/SourceBackend.Web.csproj

# Apply migration
dotnet ef database update --project src/Infrastructure/SourceBackend.Infrastructure.csproj --startup-project src/Web/SourceBackend.Web.csproj

# Remove last migration
dotnet ef migrations remove --project src/Infrastructure/SourceBackend.Infrastructure.csproj --startup-project src/Web/SourceBackend.Web.csproj

# View SQL
dotnet ef migrations script --project src/Infrastructure/SourceBackend.Infrastructure.csproj --startup-project src/Web/SourceBackend.Web.csproj
```

