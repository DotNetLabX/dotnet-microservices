using Articles.Abstractions.Enums;
using Blocks.Core;
using Microsoft.AspNetCore.Authorization;

namespace Articles.Security;

public class ArticleRoleRequirement : IAuthorizationRequirement
{
		public IReadOnlySet<UserRoleType> AllowedRoles { get; }

		public ArticleRoleRequirement(IEnumerable<string> allowedRoles)
		{
				HashSet<string> rolesSet = allowedRoles.ToHashSet(StringComparer.OrdinalIgnoreCase);
				AllowedRoles = allowedRoles.Select(r => r.ToEnum<UserRoleType>()).ToHashSet();
		}

		public ArticleRoleRequirement(IEnumerable<UserRoleType> allowedRoles)
		{				
				AllowedRoles = allowedRoles.ToHashSet();
		}
}
