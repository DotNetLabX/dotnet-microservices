namespace Review.Persistence.Repositories;

public class ArticleRepository(ReviewDbContext dbContext) 
		: Repository<Article>(dbContext)
{
		public override IQueryable<Article> Query()
		{
				return base.Entity
						.Include(e => e.Actors)
								.ThenInclude(e => e.Person)
						.Include(e => e.Assets);
		}

		public async Task<Article?> GetFullArticleByIdAsync(int id)
		{
				return await Query()
						.Include(e => e.Journal)
						.Include(e => e.SubmittedBy)
						.SingleOrDefaultAsync(e => e.Id == id);
		}
}

