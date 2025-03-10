using Articles.Abstractions;
using Articles.Abstractions.Enums;

namespace Auth.Domain.Users;

public interface IUserCreationInfo : IPersonCreationInfo
{
		string? PhoneNumber { get; }
		IReadOnlyList<IUserRole> UserRoles { get; }

		int? EmployeeId { get; }
		bool IsSuperUser { get; }
}

public interface IUserRole
{
    public UserRoleType RoleId{ get; }
    public DateTime? StartDate { get; }
    public DateTime? ExpiringDate { get; }
}

