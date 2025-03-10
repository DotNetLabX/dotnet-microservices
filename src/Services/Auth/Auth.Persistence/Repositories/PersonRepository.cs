namespace Auth.Persistence.Repositories;

public class PersonRepository(AuthDbContext dbContext) 
		: RepositoryBase<AuthDbContext, Person>(dbContext)
{
		public override IQueryable<Person> Query()
				=> base.Query().Include(p => p.User);

		public async Task<Person?> GetByEmailAsync(string email, CancellationToken ct = default)
				=> await Query().SingleOrDefaultAsync(p => p.Email.NormalizedEmail == email.ToUpperInvariant(), ct);

		public async Task<Person?> GetByUserIdAsync(int userId, CancellationToken ct = default)
				=> await Query().SingleOrDefaultAsync(p => p.UserId == userId, ct);
}
