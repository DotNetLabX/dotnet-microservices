namespace Production.Persistence.Repositories;

public class ArticleRepository(ProductionDbContext dbContext) 
		: Repository<Article>(dbContext)
{
		public override IQueryable<Article> Query()
		{
				return base.Entity
						.Include(e => e.Contributors);
		}

		public async Task<Article> GetArticleAssetsById(int id, bool throwIfNotFound = true)
		{
				return await Entity
						.Include(e => e.Assets)
								.ThenInclude(e => e.Files)
						.SingleOrThrowAsync(e => e.Id == id);
		}

		public async Task<Article> GetArticleSummaryById(int id, bool throwIfNotFound = true)
		{
				return await Entity
						.Include(e => e.Journal)
						.Include(e => e.Contributors)
								.ThenInclude(e => e.Person)
						.SingleOrThrowAsync(e => e.Id == id);

		}

		public async Task<Article> GetByIdWithAssetsAsync(int id, bool throwIfNotFound = true)
		{
				return await Query()
						 .Include(e => e.Assets)
								 //.ThenInclude(e => e.TypeDefinition)
						 .Include(e => e.Assets)
								 .ThenInclude(e => e.CurrentFileLink)
										.ThenInclude(e => e.File)

						.SingleOrThrowAsync(e => e.Id == id);				
		}
}

