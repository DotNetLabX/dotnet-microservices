namespace Auth.Domain.Users;

public partial class UserRole
{
		public static UserRole Create(IUserRole userRoleInfo)
		{
				var now = DateTime.UtcNow.Date;

				var start = userRoleInfo.StartDate ?? now;
				var end = userRoleInfo.ExpiringDate;

				if (start < now)
						throw new ArgumentException("Start date must be today or in the future.", nameof(userRoleInfo.StartDate));

				if (end.HasValue && start >= end.Value)
						throw new ArgumentException("End date must be after start date.", nameof(userRoleInfo.ExpiringDate));

				return new UserRole
				{
						RoleId = (int)userRoleInfo.RoleId,
						StartDate = start,
						ExpiringDate = end
				};
		}
}
