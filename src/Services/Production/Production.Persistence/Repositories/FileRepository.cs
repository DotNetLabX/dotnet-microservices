namespace Production.Persistence.Repositories;

public class FileRepository(ProductionDbContext _dbContext) 
    : Repository<File>(_dbContext)
{
    public override IQueryable<File> Query()
    {
				return base.Entity
						.Include(x => x.Asset);
    }

		public async Task<File> GetByIdAsync(int articleId, int fileId, bool throwNotFound = true)
				=> await Query()
						.SingleOrThrowAsync(e => e.Id == fileId && e.Asset!.ArticleId == articleId);
}
