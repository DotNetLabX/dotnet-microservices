using Auth.Domain.Users;

namespace Auth.API.Features.Users.CreateAccount;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
		public CreateUserCommandValidator()
		{
				RuleFor(c => c.FirstName).NotEmptyWithMessage(nameof(CreateUserCommand.FirstName));
				RuleFor(c => c.LastName).NotEmptyWithMessage(nameof(CreateUserCommand.LastName));
				
				RuleFor(c => c.Email)
						.NotEmptyWithMessage(nameof(CreateUserCommand.Email))
						.EmailAddress().WithMessage("Email format is invalid.");
								
				RuleFor(c => c.UserRoles)
						.NotEmptyWithMessage(nameof(CreateUserCommand.UserRoles))
						.Must((c, roles) => AreUserRoleDatesValid(roles)).WithMessage("Invalid Role");
		}

		public static bool AreUserRoleDatesValid(IReadOnlyList<UserRoleDto> roles)
		{
				return roles.All(role =>
						// StartDate must be today or in the future otherwise we might have a security concern
						(!role.StartDate.HasValue || role.StartDate.Value.Date >= DateTime.UtcNow.Date) &&

						// ExpiringDate must be after StartDate (or today if StartDate is null)
						(!role.ExpiringDate.HasValue || (role.StartDate ?? DateTime.UtcNow).Date < role.ExpiringDate.Value.Date)
				);
		}
}
