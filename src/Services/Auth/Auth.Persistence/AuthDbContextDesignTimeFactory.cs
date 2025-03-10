namespace Auth.Persistence;

public sealed class AuthDbContextDesignTimeFactory : DesignTimeFactoryBase<AuthDbContext>
{
		protected override void ConfigureProvider(DbContextOptionsBuilder<AuthDbContext> b, string cs) 
				=> b.UseSqlServer(cs);		
}