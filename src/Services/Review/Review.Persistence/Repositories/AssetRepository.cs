using Microsoft.Extensions.Caching.Memory;
using Blocks.Core.Cache;
//using AssetTypeDefinition = Review.Domain.Assets.AssetTypeDefinition;
using Review.Domain.Assets;

namespace Review.Persistence.Repositories;

public class AssetRepository(ReviewDbContext _dbContext, IMemoryCache _cache)
		: Repository<Asset>(_dbContext)
{
		public override IQueryable<Asset> Query()
		{
				return base.Entity
						.Include(x => x.Article);
		}

		public async Task<Asset?> GetByTypeAsync(int articleId, AssetType assetTypeId)
				=> await Query()
						.SingleOrDefaultAsync(e => e.ArticleId == articleId && e.Type == assetTypeId);

		public async Task<Asset?> GetByIdAsync(int articleId, int assetId)
				=> await Query()
						.SingleOrDefaultAsync(e => e.ArticleId == articleId && e.Id == assetId);

		public IEnumerable<AssetTypeDefinition> GetAssetTypes()
				=> _cache.GetOrCreateByType(entry => _dbContext.AssetTypes.AsNoTracking().ToList());

		public AssetTypeDefinition GetAssetType(AssetType type)
				=> GetAssetTypes().Single(e => e.Id == type);
}
