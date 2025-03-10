using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Auth.Persistence;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) :
				IdentityDbContext<User, Role, int>(options)
		{

		// no need to add Roles & Users here, they are already in the base class
		public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
		public virtual DbSet<Person> Persons { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

				builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
		}
}
