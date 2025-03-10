namespace Review.Persistence.Repositories;

public class ReviewerRepository(ReviewDbContext dbContext) 
		: Repository<Reviewer>(dbContext)
{
		public override IQueryable<Reviewer> Query()
		{
				return base.Entity
						.Include(e => e.Specializations);
		}

		public async Task<Reviewer?> GetByUserIdAsync(int userId, CancellationToken ct = default)
				=> await Query()
						.SingleOrDefaultAsync(e => e.UserId == userId, ct);

		public async Task<Reviewer?> GetByEmailAsync(string email, CancellationToken ct = default)
				=> await Query()
						.SingleOrDefaultAsync(e => e.Email.Value.ToLower() == email.ToLower(), ct);
}
