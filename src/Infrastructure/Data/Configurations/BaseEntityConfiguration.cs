using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SourceBackend.Core.Entities;

namespace SourceBackend.Infrastructure.Data.Configurations;

/// <summary>
/// Base configuration for entities inheriting from BaseEntity.
/// Configures common audit fields and primary key.
/// </summary>
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // Primary key
        builder.HasKey(e => e.Id);
        
        // Audit fields
        builder.Property(e => e.CreatedAt)
            .IsRequired();
        
        builder.Property(e => e.CreatedBy);
        
        builder.Property(e => e.UpdatedAt);
        
        builder.Property(e => e.UpdatedBy);
        
        builder.Property(e => e.DeletedAt);
        
        builder.Property(e => e.DeletedBy);
        
        // Optional: Add global query filter for soft deletes
        // Uncomment if you want soft-deleted entities to be automatically excluded from queries
        // builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}

