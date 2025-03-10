using Microsoft.EntityFrameworkCore;
using Articles.Security;

namespace Review.Application;

public class ArticleAccessChecker(ReviewDbContext _dbContext) : IArticleAccessChecker
{
		public async Task<bool> HasAccessAsync(int? articleId, int? userId, IReadOnlySet<UserRoleType> roles, CancellationToken ct = default)
		{				
				if (articleId is null) //the endpoint is not article related
						return true;

				if (userId is null || roles.IsNullOrEmpty())
						return false;

				if (roles.Contains(UserRoleType.EOF)) //editorial admins have access to all articles
						return true;

				return await _dbContext.ArticleActors
						.AnyAsync(e => e.ArticleId == articleId && e.Person.UserId == userId);
		}
}
