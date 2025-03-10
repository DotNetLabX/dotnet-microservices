using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Auth.Persistence.Repositories;

public class UserRepository(AuthDbContext dbContext) 
    : RepositoryBase<AuthDbContext, User>(dbContext)
{
    public override IQueryable<User> Query()
        => base.Query()
            .Include(u => u.Person);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await Query()
            .SingleOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        => await Query()
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.RefreshTokens.Any(rt => 
                rt.Token == refreshToken &&
								u.RefreshTokens.Any(rt =>
								    rt.Token == refreshToken &&
								    rt.RevokedOn == null &&
								    rt.ExpiresOn > DateTime.UtcNow)), 
            ct);


    public async Task<bool> ExistsAsync(string email, CancellationToken ct = default)
        => await Query()
            .AnyAsync(u => u.Email == email, ct);
}
