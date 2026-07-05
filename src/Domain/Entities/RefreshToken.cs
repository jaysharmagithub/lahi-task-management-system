namespace TaskManagement.Domain.Entities;

/// <summary>JWT refresh token with rotation support.</summary>
public class RefreshToken : BaseEntity
{
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? RevokedReason { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;
}
