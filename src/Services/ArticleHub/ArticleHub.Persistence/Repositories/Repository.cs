using Blocks.Entitities;

namespace ArticleHub.Persistence.Repositories;
public class Repository<TEntity>(ArticleHubDbContext dbContext)
		: RepositoryBase<ArticleHubDbContext, TEntity>(dbContext)
				where TEntity : class, IEntity<int>;
