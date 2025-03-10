using Microsoft.EntityFrameworkCore;
using Articles.Security;

namespace Production.Application;

public class ArticleAccessChecker(ProductionDbContext _dbContext) : IArticleAccessChecker
{
		public async Task<bool> HasAccessAsync(int? articleId, int? userId, IReadOnlySet<UserRoleType> roles, CancellationToken ct = default)
		{
				// the endpoint is not article specific
				if (articleId is null)
						return true; 

				if (userId is null || roles.IsNullOrEmpty())
						return false;

				// editorial admin users have access to all articles
				if (roles.Contains(UserRoleType.POF)) 
						return true;

				return await _dbContext.ArticleContributors
						.AnyAsync(e => e.ArticleId == articleId && e.Person.UserId == userId, ct);
		}
}
