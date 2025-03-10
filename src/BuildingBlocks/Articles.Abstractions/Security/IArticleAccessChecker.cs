using Articles.Abstractions.Enums;

namespace Articles.Security;

public interface IArticleAccessChecker
{
		Task<bool> HasAccessAsync(int? articleId, int? userId, IReadOnlySet<UserRoleType> roles, CancellationToken ct = default);
}
