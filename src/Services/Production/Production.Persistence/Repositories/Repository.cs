using Blocks.Entitities;

namespace Production.Persistence.Repositories;

public class Repository<TEntity>(ProductionDbContext dbContext) 
		: RepositoryBase<ProductionDbContext, TEntity>(dbContext)
				where TEntity : class, IEntity<int>
{
}
