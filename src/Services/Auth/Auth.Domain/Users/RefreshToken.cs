namespace Auth.Domain.Users;

public class RefreshToken : Entity
{
    public int UserId { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresOn { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
    public DateTime CreatedOn { get; set; }
    public required string CreatedByIp { get; set; }
    public DateTime? RevokedOn { get; set; }
    public string? RevokedByIp { get; set; } = null!;
    public string? ReplacedByToken { get; set; }
    public bool IsActive => RevokedOn is null && !IsExpired;
}
