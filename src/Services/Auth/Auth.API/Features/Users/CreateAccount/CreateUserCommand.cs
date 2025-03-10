using Articles.Abstractions.Enums;

namespace Auth.API.Features.Users.CreateAccount;

public class CreateUserCommand: IUserCreationInfo
{
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
		public required Gender Gender { get; init; }

		public Honorific? Honorific { get; init; }
		public string? PhoneNumber { get; init; }
		public string? PictureUrl { get; init; }
		public string? CompanyName { get; init; }
		public string? Position { get; init; }
		public string? Affiliation { get; init; }

		public required IReadOnlyList<UserRoleDto> UserRoles { get; init; } = new List<UserRoleDto>();

    public int? EmployeeId { get; init; }
		public bool IsSuperUser { get; init; } = false;

		IReadOnlyList<IUserRole> IUserCreationInfo.UserRoles => UserRoles;
}

public record UserRoleDto(
		UserRoleType RoleType,
		DateTime? StartDate,
		DateTime? ExpiringDate
) : IUserRole
{
		public UserRoleType RoleId => RoleType;
}

public record CreateUserResponse(
		string Email, 
		int UserId, 
		string Token
);
