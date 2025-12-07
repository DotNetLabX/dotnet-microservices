using Microsoft.AspNetCore.Identity;

namespace Auth.Domain.Users;

public partial class UserRole : IdentityUserRole<int>
{
    public DateTime StartDate { get; init; } = DateTime.UtcNow.Date;
    public DateTime? ExpiringDate { get; set; }
}
