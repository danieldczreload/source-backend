namespace SourceBackend.Core.Entities;

/// <summary>
/// Base entity with audit fields for all domain entities.
/// Includes soft delete support via DeletedAt/DeletedBy.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    
    // Audit fields
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}

