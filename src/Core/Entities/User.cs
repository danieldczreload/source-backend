namespace SourceBackend.Core.Entities;

/// <summary>
/// User entity representing system users.
/// </summary>
public sealed class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? ProfilePhoto { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastLogin { get; set; }
}

