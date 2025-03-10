namespace Auth.API.Features.Users.SetPassword;

public class SetPasswordValidator : AbstractValidator<SetPasswordCommand>
{
		public SetPasswordValidator()
		{
				RuleFor(c => c.Email).NotEmpty().EmailAddress();
				RuleFor(c => c.NewPassword).NotEmpty();
				RuleFor(c => c.ConfirmPassword).NotEmpty();
				RuleFor(c => c.TwoFactorToken).NotEmpty();
				RuleFor(c => c.NewPassword).Must(
						(command, value) => command.NewPassword == command.ConfirmPassword)
						.WithMessage("Passwords doesn't match");
		}
}

