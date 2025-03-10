using Production.Domain.Assets;

namespace Production.Persistence.Repositories;

public class AssetRepository(ProductionDbContext _dbContext) 
    : Repository<Asset>(_dbContext)
{
    public override IQueryable<Asset> Query()
    {
        return base.Entity
						.Include(x => x.Article)
						.Include(e => e.CurrentFileLink)
                .ThenInclude(e => e.File);
    }

		public async Task<Asset> GetByIdAsync(int articleId, int assetId, bool throwNotFound = true)
				=> await Query()
						.SingleOrThrowAsync(e => e.ArticleId == articleId && e.Id == assetId);
}
