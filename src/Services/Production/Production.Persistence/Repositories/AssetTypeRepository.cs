using Articles.Abstractions.Enums;
using Microsoft.Extensions.Caching.Memory;
using Production.Domain.Assets;

namespace Production.Persistence.Repositories;

public class AssetTypeRepository(ProductionDbContext dbContext, IMemoryCache cache)
		: CachedRepository<ProductionDbContext, AssetTypeDefinition, AssetType>(dbContext, cache);
