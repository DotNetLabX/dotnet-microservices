namespace Auth.API.Features.Users.Login;

public class LoginCommandValidator : Validator<LoginCommand>
{
		public LoginCommandValidator()
		{
				RuleFor(c => c.Email).NotEmpty().EmailAddress();
				RuleFor(c => c.Password).NotEmpty();
		}
}
