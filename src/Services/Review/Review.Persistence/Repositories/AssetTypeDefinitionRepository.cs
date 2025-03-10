using Microsoft.Extensions.Caching.Memory;
using Review.Domain.Assets;

namespace Review.Persistence.Repositories;

public class AssetTypeDefinitionRepository(ReviewDbContext dbContext, IMemoryCache cache)
		: CachedRepository<ReviewDbContext, AssetTypeDefinition, AssetType>(dbContext, cache);

