namespace Submission.Persistence.Repositories;

public class PersonRepository(SubmissionDbContext dbContext) 
		: Repository<Person>(dbContext)
{
		public async Task<Person?> GetByUserIdAsync(int userId)
				=> await Query()
						.SingleOrDefaultAsync(e => e.UserId == userId);

		public async Task<Person?> GetByEmailAsync(string email)
				=> await Query()
						.SingleOrDefaultAsync(e => e.Email.Value.ToLower() == email.ToLower()); //not all databases are case-insensitive
}

