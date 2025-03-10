using Articles.Abstractions.Enums;
using Blocks.AspNetCore;
using Microsoft.AspNetCore.Authorization;

namespace Articles.Security;

public class ArticleAccessAuthorizationHandler(HttpContextProvider _httpProvider, IArticleAccessChecker _articleRoleChecker)
		: AuthorizationHandler<ArticleRoleRequirement>
{
		protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ArticleRoleRequirement requirement)
		{
				var userRoles = _httpProvider.GetUserRoles<UserRoleType>()
													.Where(requirement.AllowedRoles.Contains)
													.ToHashSet();

				if (userRoles.Count > 0 
						&& await HasUserRoleForArticle(userRoles))
				{
						context.Succeed(requirement);
				}
		}

		private async Task<bool> HasUserRoleForArticle(IReadOnlySet<UserRoleType> userRoles)
		{
				return await _articleRoleChecker
						.HasAccessAsync(_httpProvider.GetArticleId(), _httpProvider.GetUserId(), userRoles, _httpProvider.HttpContext.RequestAborted);
		}
}
