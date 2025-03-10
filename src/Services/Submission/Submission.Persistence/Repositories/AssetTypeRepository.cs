using Microsoft.Extensions.Caching.Memory;

namespace Submission.Persistence.Repositories;

public class AssetTypeRepository(SubmissionDbContext dbContext, IMemoryCache cache)
		: CachedRepository<SubmissionDbContext, AssetTypeDefinition, AssetType>(dbContext, cache);