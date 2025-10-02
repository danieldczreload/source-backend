using Microsoft.EntityFrameworkCore;
using SourceBackend.Core.Entities;

namespace SourceBackend.Infrastructure.Data;

/// <summary>
/// Main application database context.
/// Configures entities and relationships for the database.
/// </summary>
public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Additional configurations can go here
    }
}

