namespace Submission.Persistence.Repositories;

public class ArticleRepository(SubmissionDbContext dbContext) 
		: Repository<Article>(dbContext)
{
		public override IQueryable<Article> Query()
		{
				return base.Entity
						.Include(e => e.Actors)
								.ThenInclude(e => e.Person)
						.Include(e => e.Assets);
		}

		public async Task<Article?> GetFullArticleByIdAsync(int id, CancellationToken ct = default)
		{
				return await Query()
						.Include(e => e.Journal)
						.Include(e => e.SubmittedBy)
						.SingleOrDefaultAsync(e => e.Id == id, ct);
		}
}

