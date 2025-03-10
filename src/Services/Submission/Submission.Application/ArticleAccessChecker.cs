using Articles.Security;
using Microsoft.EntityFrameworkCore;

namespace Submission.Application;

public class ArticleAccessChecker(SubmissionDbContext _dbContext) : IArticleAccessChecker
{
		public async Task<bool> HasAccessAsync(int? articleId, int? userId, IReadOnlySet<UserRoleType> roles, CancellationToken ct = default)
		{				
				if (articleId is null) //the endpoint is not article specific
						return true; 

				if (userId is null || roles.IsNullOrEmpty()) 
						return false;
				
				if(roles.Overlaps([UserRoleType.EOF, UserRoleType.REVED])) //editorial admin and edito users have access to all articles
						return true;

				return await _dbContext.ArticleActors
						.AsNoTracking()
						.AnyAsync(e => e.ArticleId == articleId && e.Person.UserId == userId, ct);
		}
}
