namespace Blocks.AspNetCore;

public interface IUserClaimsProvider
{
    public string UserRole { get; }
}
