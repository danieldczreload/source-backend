using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SourceBackend.Core.Entities;

namespace SourceBackend.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for User entity.
/// </summary>
public sealed class UserConfiguration : BaseEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        // Apply base configuration (Id, audit fields)
        base.Configure(builder);
        
        // Table name
        builder.ToTable("users");
        
        // Email - unique and required
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.HasIndex(e => e.Email)
            .IsUnique();
        
        // Password hash - required
        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);
        
        // Profile photo - optional
        builder.Property(e => e.ProfilePhoto)
            .HasMaxLength(500);
        
        // IsActive - required with default
        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        // LastLogin - optional
        builder.Property(e => e.LastLogin);
    }
}

