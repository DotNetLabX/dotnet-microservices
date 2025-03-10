namespace ArticleHub.Persistence.Repositories;

public class ArticleRepository(ArticleHubDbContext dbContext) 
		: RepositoryBase<ArticleHubDbContext, Article>(dbContext);
